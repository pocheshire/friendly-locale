﻿namespace FriendlyLocale.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FriendlyLocale.Interfaces;
    using FriendlyLocale.Parser;

    public class I18NProvider : II18N
    {
        public static string YamlFileExtension = "yaml";

        public I18NProvider(ITranslateContentClient contentClient)
        {
            this.ContentClient = contentClient;
            this.Locales = this.ContentClient.GetLocales();
        }

        private IList<ILocale> Locales { get; }

        private ITranslateContentClient ContentClient { get; }

        private YParser Parser { get; set; }

        public string FallbackLocale { get; set; }

        public string Translate(string[] innerKeys, string fallback)
        {
            return this.Parser?.FindValue(innerKeys) ?? fallback;
        }

        public string Translate(string key, string fallback)
        {
            return this.Parser?.FindValue(key) ?? fallback;
        }

        public ILocale CurrentLocale { get; private set; }

        public IEnumerable<ILocale> GetAvailableLocales()
        {
            return this.Locales;
        }

        public ILocale GetLocale(string locale)
        {
            return this.Locales?.FirstOrDefault(x => x.Key == locale);
        }

        public async Task ChangeLocale(ILocale locale, IProgress<float> progress)
        {
            try
            {
                var content = await this.ContentClient.GetContent(locale, progress).ConfigureAwait(false);
                this.Parser = new YParser(YParser.YParserConfig.Default, content);
                this.CurrentLocale = locale;
            }
            catch (Exception)
            {
                if (locale?.Key == this.FallbackLocale)
                {
                    throw;
                }

                var fallbackLocale = this.GetLocale(this.FallbackLocale);
                if (fallbackLocale == null)
                {
                    throw;
                }

                await this.ChangeLocale(fallbackLocale, progress);
            }
        }

        public Task ChangeLocale(string locale, IProgress<float> progress)
        {
            return this.ChangeLocale(this.GetLocale(locale), progress);
        }
    }
}