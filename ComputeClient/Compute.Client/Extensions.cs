﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DD.CBU.Compute.Api.Client
{
    public static class Extensions
    {

        public static string ToQueryString(this NameValueCollection collection)
        {
            return string.Join("&", collection.AllKeys.Where(key => !string.IsNullOrWhiteSpace(collection[key])).Select(key => string.Format("{0}={1}", WebUtility.UrlEncode(key), WebUtility.UrlEncode(collection[key]))));


        }

        private static CultureInfo ci = new CultureInfo("en-US");
        //Convert all first latter
        public static string ToTitleCase(this string str)
        {
            str = str.ToLower();
            var strArray = str.Split(' ');
            if (strArray.Length > 1)
            {
                strArray[0] = ci.TextInfo.ToTitleCase(strArray[0]);
                return string.Join(" ", strArray);
            }
            return ci.TextInfo.ToTitleCase(str);
        }
        public static string ToTitleCase(this string str, TitleCase tcase)
        {
            str = str.ToLower();
            switch (tcase)
            {
                case TitleCase.First:
                    var strArray = str.Split(' ');
                    if (strArray.Length > 1)
                    {
                        strArray[0] = ci.TextInfo.ToTitleCase(strArray[0]);
                        return string.Join(" ", strArray);
                    }
                    break;
                case TitleCase.All:
                    return ci.TextInfo.ToTitleCase(str);
                default:
                    break;
            }
            return ci.TextInfo.ToTitleCase(str);
        }
    }

    public enum TitleCase
    {
        First,
        All
    }


}

