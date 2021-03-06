using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Routing;

namespace WebApi.Hal
{
    public class Link
    {
        public Link()
        { }

        public Link(string rel, string href, string title = null)
        {
            Rel = rel;
            Href = href;
            Title = title;
        }

        public string Rel { get; set; }
        public string Href { get; set; }
        public string Title { get; set; }
        public bool IsTemplated
        {
            get { return !string.IsNullOrEmpty(Href) && IsTemplatedRegex.IsMatch(Href); }
        }

        private static readonly Regex IsTemplatedRegex = new Regex(@"{.+}", RegexOptions.Compiled);

        /// <summary>
        /// If this link is templated, you can use this method to make a non templated copy
        /// </summary>
        /// <param name="newRel">A different rel</param>
        /// <param name="parameters">The parameters, i.e 'new {id = "1"}'</param>
        /// <returns>A non templated link</returns>
        public Link CreateLink(string newRel, params object[] parameters)
        {
            return new Link(newRel, CreateUri(parameters).ToString());
        }

        /// <summary>
        /// If this link is templated, you can use this method to make a non templated copy
        /// </summary>
        /// <param name="parameters">The parameters, i.e 'new {id = "1"}'</param>
        /// <returns>A non templated link</returns>
        public Link CreateLink(params object[] parameters)
        {
            return CreateLink(Rel, parameters);
        }

        public Uri CreateUri(params object[] parameters)
        {
            var href = Href;
            href = SubstituteParams(href, parameters);

            return new Uri(href, UriKind.Relative);
        }

        public static string SubstituteParams(string href, params object[] parameters)
        {
            var uriTemplate = new UriTemplate(href);
            foreach (var parameter in parameters)
            {
                foreach (var substitution in parameter.GetType().GetProperties())
                {
                    var name = substitution.Name;
                    var value = substitution.GetValue(parameter, null);
                    var substituionValue = value == null ? null : value.ToString();
                    uriTemplate.SetParameter(name, substituionValue);
                }
            }

            return uriTemplate.Resolve();
        }
    }
}