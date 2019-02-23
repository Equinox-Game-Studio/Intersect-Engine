﻿using JetBrains.Annotations;
using Newtonsoft.Json;
using System;

namespace Intersect.Localization
{
    [Serializable]
    public class LocaleToken : LocaleNamespace
    {
        [JsonIgnore] private LocalizedString mName;

        [JsonProperty(nameof(Name), NullValueHandling = NullValueHandling.Ignore)]
        protected LocalizedString JsonName
        {
            get => mName;
            set
            {
                if (value != null && value.ToString().Length < 2)
                {
                    throw new ArgumentException(
                        $@"Token names must be at least 2 characters long, but '{value}' was provided."
                    );
                }

                mName = value;
            }
        }

        [NotNull]
        [JsonIgnore]
        public virtual LocalizedString Name
        {
            get => mName ?? throw new InvalidOperationException();
            set
            {
                if (mName == null)
                {
                    mName = value;
                }
            }
        }

        public LocaleToken()
        {
        }

        public LocaleToken([NotNull] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($@"Parameter '{nameof(name)}' cannot be null or whitespace.");
            }

            mName = name.Trim();
        }
    }
}