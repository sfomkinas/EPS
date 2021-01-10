using EPS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EPS.Helper
{
    public static class RandomProvider
    {
        private static int seed = Environment.TickCount;

        private static ThreadLocal<Random> randomWrapper = new ThreadLocal<Random>(() =>
            new Random(Interlocked.Increment(ref seed))
        );

        public static Random GetThreadRandom()
        {
            return randomWrapper.Value;
        }
    }

    public class CardCodesHelper
    {
        public static CardCode GetCardCode(EPSContext context, string code)
        {
            var queryCode = from cCode in context.CardCodes
                            where cCode.Code.ToUpper() == code.ToUpper()
                            select cCode;
            return queryCode.FirstOrDefault();
        }

        public static ChekResponse CheckCardCode(string code)
        {
            ChekResponse checkReq = new ChekResponse();
            if (string.IsNullOrWhiteSpace(code))
            {
                checkReq.Notification = "Missing CardCode";
                return checkReq;
            }
            using (var context = new EPSContext())
            {
                CardCode cardCode = GetCardCode(context, code);
                if (cardCode != null)
                {
                    var queryProducts = from pr in context.Products
                                        where pr.CardCodeID == cardCode.ID
                                        select pr.ProducCode;
                    checkReq.ProducCodes = queryProducts.ToArray();
                }
                else
                {
                    checkReq.Notification = $"No such code:{code.ToUpper()}";
                }
                return checkReq;
            }
        }

        public static UseResponse UseCode(string code)
        {
            UseResponse useResp = new UseResponse();
            if (string.IsNullOrWhiteSpace(code))
            {
                useResp.Notification = "Missing CardCode";
                useResp.UseCodeStatus = Protocol.UseCodeEnum.Error;
                return useResp;
            }

            using (var context = new EPSContext())
            {
                CardCode cardCode = GetCardCode(context, code);
                if (cardCode != null && string.IsNullOrWhiteSpace(cardCode.Used))
                {
                    cardCode.Used = DateTime.Now.ToString();
                    context.SaveChanges();
                    useResp.UseCodeStatus = Protocol.UseCodeEnum.Used;
                }
                else if (!string.IsNullOrWhiteSpace(cardCode.Used))
                {
                    useResp.Notification = $"Card code:{code.ToUpper()} alredy used";
                    useResp.UseCodeStatus = Protocol.UseCodeEnum.AlreadyUsed;
                }
                else
                {
                    useResp.Notification = $"No such code:{code.ToUpper()}";
                    useResp.UseCodeStatus = Protocol.UseCodeEnum.NoCode;
                }

                return useResp;
            }
        }

        public static string RandomCode(int length)
        {
            var charArray = "ABCDEFGHJKLMNPQRSTUVWXYZ234567892345678923456789".ToArray();
            return new string(new char[length].Select(_ => charArray[RandomProvider.GetThreadRandom().Next(charArray.Length)]).ToArray());
        }

        public static bool isUnique(EPSContext context, string code)
        {
            return !context.CardCodes.Select(_ => _.Code).Contains<string>(code);
        }

        public static string UniqueCode(EPSContext context, int length)
        {
            while (true)
            {
                var code = RandomCode(length);
                if (isUnique(context, code)) return code;
            }
        }

        public static GenerateResponse GenerateCodes(int coudeAmmount, int length)
        {
            using (var context = new EPSContext())
            {
                GenerateResponse resp = new GenerateResponse();
                try
                {
                    List<CardCode> cardCodes = new List<CardCode>();
                    for (int i = 0; i < coudeAmmount; i++)
                    {
                        var cardCode = UniqueCode(context, length);
                        cardCodes.Add(new CardCode() { Code = cardCode });
                    }

                    context.CardCodes.AddRange(cardCodes);
                    context.SaveChanges();
                    resp.GenerateStatus = Protocol.CodeGenerateEnum.Generated;
                    return resp;
                }
                catch (Exception e)
                {
                    resp.Notification = "Error generating Codes";
                    resp.GenerateStatus = Protocol.CodeGenerateEnum.Error;
                    return resp;
                }
            }
        }
    }
}
