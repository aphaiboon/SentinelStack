using System.Xml;
using System.Xml.Linq;
using SentinelStack.Application.Common.Interfaces;

namespace SentinelStack.Infrastructure.Services;

public class SamlMetadataParser : ISamlMetadataParser
{
    public SamlMetadataResult ParseMetadata(string metadataXml)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(metadataXml))
            {
                return new SamlMetadataResult
                {
                    IsValid = false,
                    ErrorMessage = "Metadata XML is required"
                };
            }

            var doc = XDocument.Parse(metadataXml);
            var ns = XNamespace.Get("urn:oasis:names:tc:SAML:2.0:metadata");
            var dsNs = XNamespace.Get("http://www.w3.org/2000/09/xmldsig#");

            // Extract EntityDescriptor entityID attribute
            var entityDescriptor = doc.Descendants(ns + "EntityDescriptor").FirstOrDefault();
            if (entityDescriptor == null)
            {
                return new SamlMetadataResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid metadata: EntityDescriptor not found"
                };
            }

            var entityId = entityDescriptor.Attribute("entityID")?.Value;
            if (string.IsNullOrEmpty(entityId))
            {
                return new SamlMetadataResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid metadata: entityID attribute not found"
                };
            }

            // Extract SingleSignOnService Location (SSO URL)
            var ssoService = entityDescriptor
                .Descendants(ns + "SingleSignOnService")
                .FirstOrDefault(e => e.Attribute("Binding")?.Value == "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");

            if (ssoService == null)
            {
                return new SamlMetadataResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid metadata: SingleSignOnService with HTTP-POST binding not found"
                };
            }

            var ssoUrl = ssoService.Attribute("Location")?.Value;
            if (string.IsNullOrEmpty(ssoUrl))
            {
                return new SamlMetadataResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid metadata: SSO Service Location not found"
                };
            }

            // Extract signing certificate
            var keyDescriptor = entityDescriptor
                .Descendants(ns + "KeyDescriptor")
                .FirstOrDefault(e => e.Attribute("use")?.Value == "signing" || e.Attribute("use") == null);

            if (keyDescriptor == null)
            {
                return new SamlMetadataResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid metadata: KeyDescriptor for signing not found"
                };
            }

            var certificate = keyDescriptor
                .Descendants(dsNs + "X509Certificate")
                .FirstOrDefault()?.Value;

            if (string.IsNullOrEmpty(certificate))
            {
                return new SamlMetadataResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid metadata: X509Certificate not found"
                };
            }

            // Clean up certificate (remove whitespace/newlines)
            certificate = certificate.Replace("\n", "").Replace("\r", "").Replace(" ", "");

            return new SamlMetadataResult
            {
                IsValid = true,
                IdpEntityId = entityId,
                SsoServiceUrl = ssoUrl,
                SigningCertificate = certificate
            };
        }
        catch (XmlException ex)
        {
            return new SamlMetadataResult
            {
                IsValid = false,
                ErrorMessage = $"Invalid XML: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new SamlMetadataResult
            {
                IsValid = false,
                ErrorMessage = $"Failed to parse metadata: {ex.Message}"
            };
        }
    }
}
