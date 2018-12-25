/*  https://github.com/hilodev/NounBase  */
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NounBase
{
    public interface ITokenService
    {
        Key GetKey(string label);
        Token GetToken(string label, Token holonym = null);
        Token GetToken(Key key, Token holonym = null);
        Token Find(string path);
        void Save();
    }
    public class TokenService : ITokenService
    {
        public readonly string TokenPathDelimiter = @"\";
        private readonly IConfigurationRoot config;
        private readonly ILogger<ITokenService> logger;
        private readonly TokenContext context;
        public TokenService(ILoggerFactory loggerFactory, IConfigurationRoot configurationRoot, TokenContext sqliteConsoleContext)
        {
            logger = loggerFactory.CreateLogger<TokenService>();
            config = configurationRoot;
            context = sqliteConsoleContext;
        }
        public Key GetKey(string label)
        {
            label = label.Replace(TokenPathDelimiter, "/").Trim();
            var key = context.Keys.ToArray().Where(_ => _.Label.Equals(label, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (key == null)
            {
                key = new Key()
                {
                    Label = label,
                    TokenType = label.IsSingular() ? KeyTypesEnum.SingularProper : KeyTypesEnum.PluralCollective,
                    ValueType = ValueTypesEnum.ProperNounIdentityText
                };
            }
            return key;
        }
        public Token GetToken(string label, Token holonym = null)
        {
            return GetToken(GetKey(label), holonym);
        }
        public Token GetToken(Key key, Token holonym=null)
        {
            var token = context.Tokens.ToArray().Where(_ => _.Key.Id == key.Id && (_.Parent == holonym)).FirstOrDefault();
            if (token == null)
            {
                token = new Token()
                {
                    Parent = holonym,
                    Key = key
                };
                context.Tokens.Add(token);
                if (holonym != null) holonym.Children.Add(token);
                context.SaveChanges();
            }
            return token;
        }
        public void Save()
        {
            context.SaveChanges();
        }
        public Token Find(string tokenPath)
        {
            while (!string.IsNullOrEmpty(tokenPath) && tokenPath.StartsWith(TokenPathDelimiter, StringComparison.CurrentCulture)) { tokenPath = tokenPath.Substring(1); }
            while (!string.IsNullOrEmpty(tokenPath) && tokenPath.EndsWith(TokenPathDelimiter, StringComparison.CurrentCulture)) { tokenPath = tokenPath.Substring(0, tokenPath.Length - 1); }
            tokenPath = tokenPath.Trim();
            if (string.IsNullOrWhiteSpace(tokenPath)) { return null; }
            var tokens = tokenPath.Split(new char[] { '\\'});
            if (tokens.Length == 0) { return null; }
            int index = 0;
            Token token = null;
            while (index < tokens.Length)
            {
                var label = tokens[index];//.IsSingular() ? tokens[index] : tokens[index].ToSingularNoun();
                var key = GetKey(label);
                token = GetToken(key, token);
                index++;
            }
            return token;
        }
    }
}