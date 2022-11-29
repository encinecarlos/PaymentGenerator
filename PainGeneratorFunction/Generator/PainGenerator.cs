﻿using Bogus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace PainGeneratorFunction.Generator
{
    public class PainGenerator
    {
        public string Generate(GeneratorRequest request)
        {
            var faker = new Faker("en");

            var ibanList = new List<string> {
                "CH0608750063000104001",
                "CH5908750063000104017",
                "CH4308750063000104014",
                "CH7608750063000104002",
                "CH3208750063000104018",
                "CH4908750063000104003",
                "CH8608750063000104016",
                "CH1608750063000104015",
                "CH3308750063000104000",
                "CH3808750063000104007",
                "CH5408750063000104010",
                "CH3308750063043754000",
                "CH0608750063043754001",
                "CH7608750063043754002",
                "CH4908750063043754003",
                "CH2008750063040564001",
                "CH9008750063040564002",
                "CH6308750063040564003",
                "CH4708750063040564000",
                "CH5008750063040394002",
                "CH2308750063040394003",
                "CH9308750063040394004",
                "CH0708750063040394000",
                "CH3208750063086454000",
                "CH0508750063086454001",
                "CH3008750050639454000",
                "CH0308750050639454001",
                "CH7308750050639454002",
                "CH2708750063000104011",
                "CH9708750063000104012",
                "CH7008750063000104013",
                "CH7108750060149374002",
                "CH4408750060149374003",
                "CH2808750060149374000",
                "CH9808750060149374001"
            };

            var paymentAmount = faker.Finance.Amount(100, 10000);

            var paymentList = new List<PaymentInstructionInformation3CH>();

            decimal totalAmount = 0;
            int counter = 0;
            string totalTransactions = string.Empty;

            var painDocument = new Document
            {
                CstmrCdtTrfInitn = new CustomerCreditTransferInitiationV03CH
                {
                    GrpHdr = new GroupHeader32CH
                    {
                        MsgId = $"MSG-{Guid.NewGuid().ToString()[..5]}",
                        CreDtTm = DateTime.Now,
                        CtrlSumSpecified = true,
                        InitgPty = new PartyIdentification32CH_NameAndId
                        {
                            Nm = faker.Name.FirstName()
                        }
                    },
                }
            };

            Parallel.ForEach(request.Payments, payment =>
            {
                var paymentLevelCList = new List<CreditTransferTransactionInformation10CH>();
                var paymentLevelB = new PaymentInstructionInformation3CH
                {
                    PmtInfId = $"PMT-{Guid.NewGuid().ToString().Substring(0, 5)}",
                    PmtMtd = PaymentMethod3Code.TRF,
                    BtchBookgSpecified = true,
                    BtchBookg = true,
                    ReqdExctnDt = DateTime.Now,
                    Dbtr = new PartyIdentification32CH
                    {
                        Nm = faker.Name.FullName(),
                        PstlAdr = new PostalAddress6CH
                        {
                            Ctry = "CH",
                            AdrLine = new string[] {
                                faker.Address.StreetName()
                            }
                        }
                    },
                    DbtrAcct = new CashAccount16CH_IdTpCcy
                    {
                        Id = new AccountIdentification4ChoiceCH
                        {
                            Item = payment.DebtorAccount,
                        }
                    },
                    DbtrAgt = new BranchAndFinancialInstitutionIdentification4CH_BicOrClrId
                    {
                        FinInstnId = new FinancialInstitutionIdentification7CH_BicOrClrId
                        {
                            BIC = faker.Finance.Bic()
                        }
                    },
                };

                Parallel.For(0, payment.CreditorQuantity, j =>
                {
                    var transaction = new CreditTransferTransactionInformation10CH
                    {
                        PmtId = new PaymentIdentification1
                        {
                            InstrId = $"INSTRID-{j + 1}-{faker.Random.AlphaNumeric(2)}",
                            EndToEndId = $"ENDTOENDID-{faker.Random.AlphaNumeric(5)}"
                        },
                        Amt = new AmountType3Choice
                        {
                            Item = new ActiveOrHistoricCurrencyAndAmount
                            {
                                Ccy = "CHF",
                                Value = paymentAmount
                            }
                        },
                        Cdtr = new PartyIdentification32CH_Name
                        {
                            Nm = faker.Name.FullName(),
                            PstlAdr = new PostalAddress6CH
                            {
                                StrtNm = faker.Address.StreetName(),
                                BldgNb = faker.Address.BuildingNumber(),
                                PstCd = faker.Address.ZipCode(),
                                TwnNm = faker.Address.CitySuffix(),
                                Ctry = "CH"
                            }
                        },
                        CdtrAcct = new CashAccount16CH_Id
                        {
                            Id = new AccountIdentification4ChoiceCH
                            {
                                Item = faker.PickRandom(ibanList)
                            }
                        },
                        RmtInf = new RemittanceInformation5CH
                        {
                            Strd = new StructuredRemittanceInformation7
                            {
                                CdtrRefInf = new CreditorReferenceInformation2
                                {
                                    Tp = new CreditorReferenceType2
                                    {
                                        CdOrPrtry = new CreditorReferenceType1Choice
                                        {
                                            Item = "QRR"
                                        }
                                    },
                                    Ref = "210000000003139471430009017"
                                },
                                AddtlRmtInf = new string[]
                                {
                                        $"ref info test {faker.Random.AlphaNumeric(5)}"
                                }
                            }
                        }

                    };

                    totalAmount += (transaction.Amt.Item as ActiveOrHistoricCurrencyAndAmount).Value;

                    paymentLevelCList.Add(transaction);
                });

                counter += paymentLevelCList.Count;

                paymentLevelB.CdtTrfTxInf = paymentLevelCList.ToArray();

                paymentList.Add(paymentLevelB);
                paymentLevelCList.Clear();
            });

            painDocument.CstmrCdtTrfInitn.GrpHdr.CtrlSum = totalAmount;
            painDocument.CstmrCdtTrfInitn.GrpHdr.NbOfTxs = counter.ToString();
            painDocument.CstmrCdtTrfInitn.PmtInf = paymentList.ToArray();

            var pain001 = new XmlSerializer(painDocument.GetType());

            using var ms = new MemoryStream();

            var xmlSettings = new XmlWriterSettings()
            {
                Encoding = new UTF8Encoding(false),
            };

            using var writer = XmlWriter.Create(ms, xmlSettings);

            pain001.Serialize(writer, painDocument);

            byte[] buffer = ms.ToArray();

            var content = Encoding.UTF8.GetString(buffer);

            var file = request.EncodeResult is true ? Convert.ToBase64String(buffer) : content;

            return file;
        }
    }
}
