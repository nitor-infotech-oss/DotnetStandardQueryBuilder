﻿namespace DotnetStandardQueryBuilder.OData
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using DotnetStandardQueryBuilder.Core;
    using System;

    public class UriParser
    {
        private readonly UriParserSettings _uriParserSettings;

        public UriParser(UriParserSettings uriParserSettings = null)
        {
            _uriParserSettings = uriParserSettings ?? UriParserSettings.Default;
        }

        public IRequest Parse<T>(string queryString)
            where T : class, new()
        {
            var requestUri = new Uri($"{nameof(T)}/{queryString}", UriKind.Relative);

            var parser = new ODataUriParser(GetEdmModel<T>(nameof(T)), requestUri)
            {
                Resolver = new ODataUriResolver()
                {
                    EnableCaseInsensitive = _uriParserSettings.EnableCaseInsensitive
                }
            };
            
            bool? count = parser.ParseCount();
            long? top = parser.ParseTop();
            long? skip = parser.ParseSkip();

            return new Request
            {
                Filter = parser.ParseFilter().Parse(),
                Select = parser.ParseSelectAndExpand().Parse(),
                Sorts = parser.ParseOrderBy().Parse(),
                Count = count.HasValue ? Convert.ToBoolean(count.Value) : false,
                Page = skip.HasValue ? Convert.ToInt32(skip.Value) + 1 : 1,
                PageSize = top.HasValue ? Convert.ToInt32(top.Value) : null
            };
        }

        private static IEdmModel GetEdmModel<T>(string entityName)
            where T : class, new()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.EntitySet<T>(entityName);

            return odataBuilder.GetEdmModel();
        }
    }
}