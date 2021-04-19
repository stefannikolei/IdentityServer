// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Duende.IdentityServer.EntityFramework.Stores
{
    /// <summary>
    /// Implementation of IClientStore thats uses EF.
    /// </summary>
    /// <seealso cref="IClientStore" />
    public class OidcIdentityProviderStore : IIdentityProviderStore
    {
        /// <summary>
        /// The DbContext.
        /// </summary>
        protected readonly IConfigurationDbContext Context;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger<OidcIdentityProviderStore> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OidcIdentityProviderStore"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">context</exception>
        public OidcIdentityProviderStore(IConfigurationDbContext context, ILogger<OidcIdentityProviderStore> logger)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IdentityProvider> GetBySchemeAsync(string scheme)
        {
            var query = Context.OidcIdentityProviders.Where(x => x.Scheme == scheme);

            var idp = (await query.ToArrayAsync()).SingleOrDefault(x => x.Scheme == scheme);
            if (idp == null) return null;

            var result = MapIdp(idp);
            if (result == null)
            {
                Logger.LogError("Identity provider record found in database, but mapping failed for scheme {scheme} and protocol type {protocol}", idp.Scheme, idp.Type);
            }
            
            return result;
        }

        /// <summary>
        /// Maps from the identity provider entity to identity provider model.
        /// </summary>
        /// <param name="idp"></param>
        /// <returns></returns>
        protected virtual IdentityProvider MapIdp(Entities.OidcIdentityProvider idp)
        {
            if (idp.Type == "oidc")
            {
                return idp.ToOidcModel();
            }

            return null;
        }
    }
}