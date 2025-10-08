using System.Text.RegularExpressions;
using HeimdallWeb.Helpers;
using Newtonsoft.Json.Linq;

public static class JsonPreprocessor
{
    public static void PreProcessScanResults(ref string jsonString)
    {
        var json = jsonString.ToJson();

        // Normalizar timestamps
        NormalizeTimestamps(json);

        // Processar headers HTTP
        ProcessHttpHeaders(json);

        // Processar resultados SSL
        ProcessSslResults(json);

        // Processar portas
        ProcessPortResults(json);

        // Processar redirecionamentos
        ProcessRedirectResults(json);
    }

    private static void NormalizeTimestamps(JObject json)
    {
        var timestampPattern = @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d+Z";
        var matches = Regex.Matches(json.ToString(), timestampPattern);

        foreach (Match match in matches)
        {
            var timestamp = match.Value;
            var normalized = DateTime.Parse(timestamp).ToString("yyyy-MM-dd HH:mm:ss");
            json.ToString().Replace(timestamp, normalized);
        }
    }

    private static void ProcessHttpHeaders(JObject json)
    {
        var headers = json["headers"];
        if (headers != null)
        {
            var headersObject = headers.Value<JObject>();
            var processedHeaders = new JObject();

            foreach (JProperty property in headersObject.Properties())
            {
                if (property.Value.Type == JTokenType.String)
                {
                    processedHeaders[property.Name] = property.Value.ToString().ToLower();
                }
                else
                {
                    processedHeaders[property.Name] = property.Value;
                }
            }

            json["headers"] = processedHeaders;
        }
    }

    private static void ProcessSslResults(JObject json)
    {
        var sslResults = json["resultsSslScanner"];
        if (sslResults != null)
        {
            foreach (var result in sslResults)
            {
                var notBefore = result["notBefore"].ToString();
                var notAfter = result["notAfter"].ToString();

                result["validityPeriod"] = CalculateValidityPeriod(notBefore, notAfter);
                result["expirationStatus"] = GetExpirationStatus(notBefore, notAfter);
            }
        }
    }

    private static void ProcessPortResults(JObject json)
    {
        if (json is null) return;
        var arr = json["resultsPortScanner"] as JArray;
        if (arr is null) return;

        for (int i = arr.Count - 1; i >= 0; i--)
        {
            var item = arr[i] as JObject;
            bool isOpen = item?.Value<bool?>("open") ?? false;
            if (!isOpen)
            {
                arr.RemoveAt(i);
                continue;
            }

            // normalize / summarize banner
            item["open"] = "true"; // opcional
            if (item["banner"] != null)
                item["bannerSummary"] = SummarizeBanner(item.Value<string>("banner") ?? "");
        }
    }

    private static void ProcessRedirectResults(JObject json)
    {
        var redirectResults = json["resultsHttpRedirectScanner"];
        if (redirectResults != null)
        {
            foreach (var result in redirectResults)
            {
                if (result.Value<string>("status") == "redirect_found")
                    result["redirect_detected"] = result["redirect_detected"].ToString().ToLower();
            }
        }
    }

    private static string CalculateValidityPeriod(string notBefore, string notAfter)
    {
        var start = DateTime.Parse(notBefore);
        var end = DateTime.Parse(notAfter);
        return $"{start:yyyy-MM-dd} to {end:yyyy-MM-dd}";
    }

    private static string GetExpirationStatus(string notBefore, string notAfter)
    {
        var end = DateTime.Parse(notAfter);
        var daysRemaining = (end - DateTime.Now).Days;
        return daysRemaining > 30 ? "ok" : daysRemaining > 0 ? "warning" : "expired";
    }

    private static string SummarizeBanner(string banner)
    {
        if (string.IsNullOrEmpty(banner)) return "empty";

        return banner switch
        {
            // Serviços Web
            _ when banner.Contains("HTTP", StringComparison.OrdinalIgnoreCase) => "http",
            _ when banner.Contains("NGINX", StringComparison.OrdinalIgnoreCase) => "nginx",
            _ when banner.Contains("APACHE", StringComparison.OrdinalIgnoreCase) => "apache",
            _ when banner.Contains("IIS", StringComparison.OrdinalIgnoreCase) => "iis",
            _ when banner.Contains("LIGHTTPD", StringComparison.OrdinalIgnoreCase) => "lighttpd",
            _ when banner.Contains("NODEJS", StringComparison.OrdinalIgnoreCase) => "nodejs",
            _ when banner.Contains("TOMCAT", StringComparison.OrdinalIgnoreCase) => "tomcat",
            _ when banner.Contains("JBOSS", StringComparison.OrdinalIgnoreCase) => "jboss",
            _ when banner.Contains("WEBSPHERE", StringComparison.OrdinalIgnoreCase) => "websphere",
            _ when banner.Contains("GLASSFISH", StringComparison.OrdinalIgnoreCase) => "glassfish",

            // Serviços de Banco de Dados
            _ when banner.Contains("MYSQL", StringComparison.OrdinalIgnoreCase) => "mysql",
            _ when banner.Contains("POSTGRESQL", StringComparison.OrdinalIgnoreCase) => "postgres",
            _ when banner.Contains("MONGODB", StringComparison.OrdinalIgnoreCase) => "mongo",
            _ when banner.Contains("REDIS", StringComparison.OrdinalIgnoreCase) => "redis",
            _ when banner.Contains("ORACLE", StringComparison.OrdinalIgnoreCase) => "oracle",
            _ when banner.Contains("SQLSERVER", StringComparison.OrdinalIgnoreCase) => "mssql",
            _ when banner.Contains("DB2", StringComparison.OrdinalIgnoreCase) => "db2",
            _ when banner.Contains("FIREBIRD", StringComparison.OrdinalIgnoreCase) => "firebird",
            _ when banner.Contains("COUCHDB", StringComparison.OrdinalIgnoreCase) => "couchdb",
            _ when banner.Contains("CASSANDRA", StringComparison.OrdinalIgnoreCase) => "cassandra",

            // Serviços de Email
            _ when banner.Contains("SMTP", StringComparison.OrdinalIgnoreCase) => "smtp",
            _ when banner.Contains("IMAP", StringComparison.OrdinalIgnoreCase) => "imap",
            _ when banner.Contains("POP3", StringComparison.OrdinalIgnoreCase) => "pop3",
            _ when banner.Contains("EXIM", StringComparison.OrdinalIgnoreCase) => "exim",
            _ when banner.Contains("SENDMAIL", StringComparison.OrdinalIgnoreCase) => "sendmail",
            _ when banner.Contains("POSTFIX", StringComparison.OrdinalIgnoreCase) => "postfix",
            _ when banner.Contains("DOVECOT", StringComparison.OrdinalIgnoreCase) => "dovecot",
            _ when banner.Contains("ZIMBRA", StringComparison.OrdinalIgnoreCase) => "zimbra",
            _ when banner.Contains("LOTUS", StringComparison.OrdinalIgnoreCase) => "lotus",
            _ when banner.Contains("EXCHANGE", StringComparison.OrdinalIgnoreCase) => "exchange",

            // Serviços de Arquivos
            _ when banner.Contains("FTP", StringComparison.OrdinalIgnoreCase) => "ftp",
            _ when banner.Contains("SFTP", StringComparison.OrdinalIgnoreCase) => "sftp",
            _ when banner.Contains("SAMBA", StringComparison.OrdinalIgnoreCase) => "samba",
            _ when banner.Contains("NFS", StringComparison.OrdinalIgnoreCase) => "nfs",
            _ when banner.Contains("CIFS", StringComparison.OrdinalIgnoreCase) => "cifs",
            _ when banner.Contains("WEBDAV", StringComparison.OrdinalIgnoreCase) => "webdav",
            _ when banner.Contains("AFP", StringComparison.OrdinalIgnoreCase) => "afp",
            _ when banner.Contains("SMB", StringComparison.OrdinalIgnoreCase) => "smb",
            _ when banner.Contains("ISCSI", StringComparison.OrdinalIgnoreCase) => "iscsi",
            _ when banner.Contains("GLUSTER", StringComparison.OrdinalIgnoreCase) => "gluster",

            // Serviços de Autenticação
            _ when banner.Contains("LDAP", StringComparison.OrdinalIgnoreCase) => "ldap",
            _ when banner.Contains("KERBEROS", StringComparison.OrdinalIgnoreCase) => "kerberos",
            _ when banner.Contains("RADIUS", StringComparison.OrdinalIgnoreCase) => "radius",
            _ when banner.Contains("TACACS", StringComparison.OrdinalIgnoreCase) => "tacacs",
            _ when banner.Contains("OPENLDAP", StringComparison.OrdinalIgnoreCase) => "openldap",
            _ when banner.Contains("ACTIVE DIRECTORY", StringComparison.OrdinalIgnoreCase) => "ad",
            _ when banner.Contains("FREEIPA", StringComparison.OrdinalIgnoreCase) => "freeipa",
            _ when banner.Contains("OPENAM", StringComparison.OrdinalIgnoreCase) => "openam",
            _ when banner.Contains("KEYCLOAK", StringComparison.OrdinalIgnoreCase) => "keycloak",
            _ when banner.Contains("AUTH0", StringComparison.OrdinalIgnoreCase) => "auth0",

            // Serviços de Monitoramento
            _ when banner.Contains("PROMETHEUS", StringComparison.OrdinalIgnoreCase) => "prometheus",
            _ when banner.Contains("GRAFANA", StringComparison.OrdinalIgnoreCase) => "grafana",
            _ when banner.Contains("ZABBIX", StringComparison.OrdinalIgnoreCase) => "zabbix",
            _ when banner.Contains("NAGIOS", StringComparison.OrdinalIgnoreCase) => "nagios",
            _ when banner.Contains("SOLARWINDS", StringComparison.OrdinalIgnoreCase) => "solarwinds",
            _ when banner.Contains("NEWRELIC", StringComparison.OrdinalIgnoreCase) => "newrelic",
            _ when banner.Contains("DATADOG", StringComparison.OrdinalIgnoreCase) => "datadog",
            _ when banner.Contains("SPLUNK", StringComparison.OrdinalIgnoreCase) => "splunk",
            _ when banner.Contains("ELASTICSEARCH", StringComparison.OrdinalIgnoreCase) => "elasticsearch",
            _ when banner.Contains("KIBANA", StringComparison.OrdinalIgnoreCase) => "kibana",

            // Outros Serviços
            _ when banner.Contains("SSH", StringComparison.OrdinalIgnoreCase) => "ssh",
            _ when banner.Contains("TELNET", StringComparison.OrdinalIgnoreCase) => "telnet",
            _ when banner.Contains("SNMP", StringComparison.OrdinalIgnoreCase) => "snmp",
            _ when banner.Contains("DNS", StringComparison.OrdinalIgnoreCase) => "dns",
            _ when banner.Contains("DHCP", StringComparison.OrdinalIgnoreCase) => "dhcp",
            _ when banner.Contains("NTP", StringComparison.OrdinalIgnoreCase) => "ntp",
            _ when banner.Contains("RABBITMQ", StringComparison.OrdinalIgnoreCase) => "rabbitmq",
            _ when banner.Contains("KAFKA", StringComparison.OrdinalIgnoreCase) => "kafka",
            _ when banner.Contains("ZOOKEEPER", StringComparison.OrdinalIgnoreCase) => "zookeeper",
            _ when banner.Contains("ETCD", StringComparison.OrdinalIgnoreCase) => "etcd",
            _ when banner.Contains("CONSUL", StringComparison.OrdinalIgnoreCase) => "consul",
            _ when banner.Contains("HAPROXY", StringComparison.OrdinalIgnoreCase) => "haproxy",
            _ when banner.Contains("VARNISH", StringComparison.OrdinalIgnoreCase) => "varnish",
            _ when banner.Contains("MEMCACHED", StringComparison.OrdinalIgnoreCase) => "memcached",
            _ when banner.Contains("RSYNCD", StringComparison.OrdinalIgnoreCase) => "rsync",
            _ when banner.Contains("OPENVPN", StringComparison.OrdinalIgnoreCase) => "openvpn",
            _ when banner.Contains("PPTP", StringComparison.OrdinalIgnoreCase) => "pptp",
            _ when banner.Contains("L2TP", StringComparison.OrdinalIgnoreCase) => "l2tp",
            _ when banner.Contains("IPSEC", StringComparison.OrdinalIgnoreCase) => "ipsec",
            _ when banner.Contains("OPENSSL", StringComparison.OrdinalIgnoreCase) => "openssl",
            _ when banner.Contains("STUN", StringComparison.OrdinalIgnoreCase) => "stun",
            _ when banner.Contains("TURN", StringComparison.OrdinalIgnoreCase) => "turn",
            _ when banner.Contains("RTP", StringComparison.OrdinalIgnoreCase) => "rtp",
            _ when banner.Contains("RTSP", StringComparison.OrdinalIgnoreCase) => "rtsp",
            _ when banner.Contains("ICE", StringComparison.OrdinalIgnoreCase) => "ice",
            _ when banner.Contains("SIP", StringComparison.OrdinalIgnoreCase) => "sip",
            _ when banner.Contains("XMPP", StringComparison.OrdinalIgnoreCase) => "xmpp",
            _ when banner.Contains("IRC", StringComparison.OrdinalIgnoreCase) => "irc",
            _ when banner.Contains("GSTREAMER", StringComparison.OrdinalIgnoreCase) => "gstreamer",
            _ when banner.Contains("FFMPEG", StringComparison.OrdinalIgnoreCase) => "ffmpeg",
            _ when banner.Contains("RED5", StringComparison.OrdinalIgnoreCase) => "red5",
            _ when banner.Contains("MONGODB", StringComparison.OrdinalIgnoreCase) => "mongo",
            _ => "unknown"
        };
    }
}