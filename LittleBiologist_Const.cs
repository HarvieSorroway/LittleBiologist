using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace LittleBiologist
{
    public static class LittleBiologist_Const
    {
        public static void Log(params object[] args)
        {
            string result = "[LittleBiologist]";

            if (args.Length > 0)
            {
                foreach(object a in args)
                {
                    result += a.ToString() + ":";
                }

                result = result.Substring(0, result.Length - 1);
            }
            else
            {
                result += Time.time.ToString();
            }

            Debug.Log(result);
        }

    }
}
