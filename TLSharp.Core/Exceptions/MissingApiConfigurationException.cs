﻿using System;

namespace TLSharp.Core.Exceptions
{
    public class MissingApiConfigurationException : Exception
    {
        public const string InfoUrl = " https://github.com/cobra91/TelegramCSharpForward#quick-configuration";
       

        internal MissingApiConfigurationException(string invalidParamName) :
            base($"Your {invalidParamName} setting is missing. Adjust the configuration first, see {InfoUrl}")
        {
        }
    }
}