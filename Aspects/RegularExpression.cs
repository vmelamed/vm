using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace vm.Aspects
{
    /// <summary>
    /// Class RegexConstants.
    /// </summary>
    public static class RegularExpression
    {
        #region Fragments of regular expressions
        /// <summary>
        /// The general definition of protocol scheme. May be not very useful if you want to communicate with specific schemes.
        /// </summary>
        const string rexGeneralScheme = @"(?<schema>[a-z][a-z0-9\.+-]*):";

        /// <summary>
        /// Possible protocol schemes in the URL-s. Add more schemas if you need to (should be case insensitive!)
        /// </summary>
        const string rexScheme = "(?<schema>https?|ftp|file):";

        /// <summary>
        /// The WCF schemes which are closely related to the endpoint binding.
        /// </summary>
        const string rexWcfScheme = @"(?<schema>https?|net\.tcp|net\.pipe|net\.msmq|ws|sb):";

        /// <summary>
        /// Matches an octet (number in the closed range 0-255) from an IPv4 address, e.g. 192
        /// </summary>
        const string rexIpV4Segment = "(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])";

        /// <summary>
        /// Matches numeric IPv4 addresses, e.g. 192.168.10.113
        /// </summary>
        const string rexNumIpV4Adr = "(?<ipV4>(?:" + rexIpV4Segment + @"\.){3}" + rexIpV4Segment + ")";

        /// <summary>
        /// Matches a double octet (number in the closed range 0-ffff hexadecimal) from an IPv6 address, e.g. fdee (should be case insensitive!)
        /// </summary>
        const string rexIpV6Segment = "[0-9a-f]{1,4}";

        // IPv6 regex-s are based on http://stackoverflow.com/a/17871737/850432:
        //
        // IPV4SEG  = (25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])
        // IPV4ADDR = (IPV4SEG\.){3,3}IPV4SEG
        // IPV6SEG  = [0-9a-fA-F]{1,4}
        // IPV6ADDR = (
        //            (IPV6SEG:){7,7}IPV6SEG|                # 1:2:3:4:5:6:7:8
        //            (IPV6SEG:){1,7}:|                      # 1::                                 1:2:3:4:5:6:7::
        //            (IPV6SEG:){1,6}:IPV6SEG|               # 1::8               1:2:3:4:5:6::8   1:2:3:4:5:6::8
        //            (IPV6SEG:){1,5}(:IPV6SEG){1,2}|        # 1::7:8             1:2:3:4:5::7:8   1:2:3:4:5::8
        //            (IPV6SEG:){1,4}(:IPV6SEG){1,3}|        # 1::6:7:8           1:2:3:4::6:7:8   1:2:3:4::8
        //            (IPV6SEG:){1,3}(:IPV6SEG){1,4}|        # 1::5:6:7:8         1:2:3::5:6:7:8   1:2:3::8
        //            (IPV6SEG:){1,2}(:IPV6SEG){1,5}|        # 1::4:5:6:7:8       1:2::4:5:6:7:8   1:2::8
        //            IPV6SEG:((:IPV6SEG){1,6})|             # 1::3:4:5:6:7:8     1::3:4:5:6:7:8   1::8
        //            :((:IPV6SEG){1,7}|:)|                  # ::2:3:4:5:6:7:8    ::2:3:4:5:6:7:8  ::8       ::       
        //            fe80:(:IPV6SEG){0,4}%[0-9a-zA-Z]{1,}|  # fe80::7:8%eth0     fe80::7:8%1  (link-local IPv6 addresses with zone index)
        //            ::(ffff(:0{1,4}){0,1}:){0,1}IPV4ADDR|  # ::255.255.255.255  ::ffff:255.255.255.255  ::ffff:0:255.255.255.255 (IPv4-mapped IPv6 addresses and IPv4-translated addresses)
        //            (IPV6SEG:){1,4}:IPV4ADDR               # 2001:db8:3:4::192.0.2.33  64:ff9b::192.0.2.33 (IPv4-Embedded IPv6 Address)
        //            )

        /// <summary>
        /// Matches numeric IPv6 addresses, e.g. 1:2:3:4:5::8 (should be case insensitive!)
        /// </summary>
        const string rexNumIpV6Adr = "(?<ipV6>" +
                                        "(?:fe80:(?::" + rexIpV6Segment + "){0,4}%[0-9a-z]+)|" +
                                        "(?:::(?:ffff(?::0{1,4})?:)?" + rexNumIpV4Adr + ")|" +
                                        "(?:(?:" + rexIpV6Segment + ":){1,4}:" + rexNumIpV4Adr + ")|" +
                                        rexIpV6Segment + ":(?::" + rexIpV6Segment + "){1,6}|" +
                                        "(?:(?:" + rexIpV6Segment + ":){1,2}(?::" + rexIpV6Segment + "){1,5})|" +
                                        "(?:(?:" + rexIpV6Segment + ":){1,3}(?::" + rexIpV6Segment + "){1,4})|" +
                                        "(?:(?:" + rexIpV6Segment + ":){1,4}(?::" + rexIpV6Segment + "){1,3})|" +
                                        "(?:(?:" + rexIpV6Segment + ":){1,5}(?::" + rexIpV6Segment + "){1,2})|" +
                                        "(?:(?:" + rexIpV6Segment + ":){1,6}:" + rexIpV6Segment + ")|" +
                                        "(?:(?:" + rexIpV6Segment + ":){1,7}:)|" +
                                        "(?::(?::" + rexIpV6Segment + "){1,7})|" +
                                        "(?:(?:" + rexIpV6Segment + ":){7}" + rexIpV6Segment + ")|" +
                                        "::" +
                                     ")";

        /// <summary>
        /// Matches numeric IP (either v4 or v6) addresses for e-mails, e.g. 1:2:3:4:5::8 (should be case insensitive!)
        /// </summary>
        const string rexEmailNumIp = "(?<ipNumIp>" + rexNumIpV4Adr + "|" + rexNumIpV6Adr + ")";

        /// <summary>
        /// Matches numeric IP (either v4 or v6) addresses for URL-s, e.g. [1:2:3:4:5::8] (should be case insensitive!)
        /// </summary>
        const string rexUrlNumIp = @"(?<ipNumIp>" + rexNumIpV4Adr + @"|\[" + rexNumIpV6Adr + @"\])";

        /// <summary>
        /// Matches fragments of tertiary domain(s)- www or file_server (should be case insensitive!)
        /// </summary>
        const string rexDnsIpAdr4 = @"[\w!~*'()-]+";

        /// <summary>
        /// Matches fragments of tertiary domain(s), e.g. www[.vm.com] or laura.corp[.vm.com] (addresses without the domain part, should be case insensitive!)
        /// Note that 123.12.45.78 matches it too! In other words this is an address that can be resolved without applying the domain name.
        /// </summary>
        const string rexDnsIpAdr3 = "(?<subDomainLevel>" + rexDnsIpAdr4 + @"(?:\." + rexDnsIpAdr4 + ")*)";

        /// <summary>
        /// Matches second level domain, e.g. google, FaceBook, etc. (should be case insensitive!)
        /// </summary>
        const string rexDnsIpAdr2 = @"(?<domainLevel>(?:[\d\p{L}][\d\p{L}-]{0,61})?[\p{L}])";

        /// <summary>
        /// top level domain, e.g. com, net, etc. (should be case insensitive!)
        /// </summary>
        //const string rexDnsIpAdr1  = "(?<topLevel>aero|asia|cat|biz|com|coop|edu|gov|info|int|jobs|mil|mobi|museum|name|net|org|pro|tel|travel|xxx|[a-z]{2})";
        const string rexDnsIpAdr1 = @"(?<topLevel>\p{L}{2,32})";

        /// <summary>
        /// Matches domain name - microsoft.com
        /// </summary>
        const string rexDomainDnsIpAdr = rexDnsIpAdr2 + @"\." + rexDnsIpAdr1;

        /// <summary>
        /// Matches sub-domain name - abc123.google.com
        /// </summary>
        const string rexSubDomainOrHostDnsIpAdr = "(?:" + rexDnsIpAdr3 + @"\.)?" + rexDomainDnsIpAdr;

        /// <summary>
        /// Matches full (FQDN) IP address (should be case insensitive!) or localhost
        /// </summary>
        const string rexFullDnsIpAdr = "(?:" + rexSubDomainOrHostDnsIpAdr + ")|localhost";

        /// <summary>
        /// Matches URL IP address in either numeric or FQDN (domain is required) or "localhost" (should be case insensitive!)
        /// </summary>
        const string rexUrlFullIpAdr = rexFullDnsIpAdr + "|" + rexUrlNumIp;

        /// <summary>
        /// Matches URL IP address which allows skipping the domain portion (should be case insensitive!),
        /// </summary>
        const string rexUrlIpAdr = rexUrlFullIpAdr + "|" + rexDnsIpAdr3;

        /// <summary>
        /// Matches e-mail IP address in either numeric or FQDN (domain is required) or "localhost" (should be case insensitive!) - it is more restrictive than RexIpAdr
        /// </summary>
        const string rexFullEmailIpAdr = rexFullDnsIpAdr + "|" + rexEmailNumIp;

        /// <summary>
        /// Matches e-mail DNS IP address which allows skipping the domain portion (should be case insensitive!),
        /// </summary>
        const string rexEmailIpAdr = rexFullEmailIpAdr + "|" + rexDnsIpAdr3;

        /// <summary>
        /// Matches unsigned int16 (port)
        /// </summary>
        const string rexPort = "(?<port>6553[0-5]|655[0-2][0-9]|65[0-4][0-9][0-9]|6[0-4][0-9][0-9][0-9]|[1-5][0-9][0-9][0-9][0-9]|[1-9][0-9]{0,3})";

        /// <summary>
        /// Matches &lt;IP addr requiring domain name&gt;[:&lt;port&gt;] (should be case insensitive!)
        /// IP address in either numeric or FQDN (domain is required) or "localhost" - more restrictive than RexIpAdrPort
        /// </summary>
        const string rexUrlFullIpAdrPort = "(?<address>" + rexUrlFullIpAdr + ")" + "(?::" + rexPort + ")?";

        /// <summary>
        /// Matches &lt;IP addr&gt;[:&lt;port&gt;] (should be case insensitive!)
        /// </summary>
        const string rexUrlIpAdrPort = "(?<address>" + rexUrlIpAdr + ")" + "(?::" + rexPort + ")?";

        /// <summary>
        /// The characters in an email's user ID without the allowed dots
        /// </summary>
        const string rexUserIdAlphas = @"[0-9a-z\u007f-\uffff!#$%&'*+/=?^_`{|}~-]+";

        /// <summary>
        /// Matches the email's user ID syntax
        /// </summary>
        const string rexUserId = @"(" + rexUserIdAlphas + @"(?:\." + rexUserIdAlphas + @")*|""(?:[\x01-\x09\x0B\x0C\x0E-\x21\x23-\x5B\x5D-\x7F]|\\\\|\\"")*"")";

        /// <summary>
        /// Matches the password syntax. (not checking for password complexity).
        /// </summary>
        const string rexPassword = rexUserId;

        /// <summary>
        /// Matches optional user ID + opt.password
        /// </summary>
        const string rexUserPwd = "(?<userId>" + rexUserId + ")(?::(?<password>" + rexPassword + "))?";

        /// <summary>
        /// Matches the e-mail id portion of an SMTP e-mail address
        /// </summary>
        const string rexEmailId = "(?<userId>" + rexUserId + ")";

        const string rexXalpha  = @"[a-z0-9$_@.&+!*""'(),-]|%[0-9a-f][0-9a-f]";
        const string rexXalphas = @"(?:" + rexXalpha + ")+";
        const string rexQalpha  = @"[a-z0-9$_@.+!*""'(),-]|%[0-9a-f][0-9a-f]";
        const string rexQalphas = @"(?:" + rexQalpha + ")+";

        /// <summary>
        /// Matches a URL path segment
        /// </summary>
        const string rexPathSegment = rexXalphas;

        /// <summary>
        /// Matches a URL path
        /// </summary>
        const string rexUrlPath = "(?<path>(?:/?|(?:/" + rexPathSegment + ")*))";

        const string rexQueryParameter = rexQalphas;
        const string rexQueryValue = rexQalphas;
        const string rexQueryParamValue = rexQueryParameter + "=" + rexQueryValue;

        /// <summary>
        /// Matches the query part of a URL
        /// </summary>
        const string rexUrlQuery = @"(?<query>(?:\?" + rexQueryParamValue + "(?:(?:&|;)" + rexQueryParamValue + ")*)?)";

        /// <summary>
        /// Matches URL root with FQDN, including domain and top level portions (vm.com), or IP address, or localhost.
        /// </summary>
        const string rexFullUrlRoot = "(?:" + rexScheme + "//)" + "(?:" + rexUserPwd + "@)?(?<host>" + rexUrlFullIpAdrPort + ")";

        /// <summary>
        /// Matches URL root including intranet addresses (without domain and top level parts). 
        /// </summary>
        const string rexUrlRoot = "(?:" + rexScheme + "//)" + "(?:" + rexUserPwd + "@)?(?<host>" + rexUrlIpAdrPort + ")";

        /// <summary>
        /// Matches URL root with FQDN, i.e. all levels of DNS level or IP address or localhost. Allows for WCF schemes.
        /// </summary>
        const string rexFullWcfUrlRoot = "(?:" + rexWcfScheme + "//)" + "(?:" + rexUserPwd + "@)?(?<host>" + rexUrlFullIpAdrPort + ")";

        /// <summary>
        /// Matches URL root including intranet addresses without domain and top level parts. On the internet these addresses may fail. Allows for WCF schemes.
        /// </summary>
        const string rexWcfUrlRoot = "(?:" + rexWcfScheme + "//)" + "(?:" + rexUserPwd + "@)?(?<host>" + rexUrlIpAdrPort + ")";

        /// <summary>
        /// Matches URL root and path with FQDN, including domain and top level portions (vm.com), or IP address, or localhost.
        /// </summary>
        const string rexFullUrlRootAndPath = rexFullUrlRoot + rexUrlPath;

        /// <summary>
        /// Matches URL root and path including intranet addresses (without domain and top level parts). 
        /// Note: in internet scenario these addresses may fail to connect to the resource, as they may be missing the top and domain level parts of the addresses, e.g. missing vm.com.
        /// </summary>
        const string rexUrlRootAndPath = rexUrlRoot + rexUrlPath;

        /// <summary>
        /// Matches URL root and path requiring FQDN, i.e. all levels of DNS level or IP address or localhost.
        /// Allows for WCF schemes.
        /// Note: requires fully qualified IP addresses like abc.vm.com or localhost or 22.33.44.55. Therefore this one is more restrictive - e.g. abc.123 will not match but abc.123.vm.com will.
        /// </summary>
        const string rexFullWcfUrlRootAndPath = rexFullWcfUrlRoot + rexUrlPath;

        /// <summary>
        /// Matches URL root and path including intranet addresses without domain and top level parts. On the internet these addresses may fail.
        /// Allows for WCF schemes.
        /// Note: in internet scenario these addresses may fail to connect to the resource, as they may be missing the top and domain level parts of the addresses, e.g. missing vm.com.
        /// </summary>
        const string rexWcfUrlRootAndPath = rexWcfUrlRoot + rexUrlPath;
        #endregion

        #region Internet addresses, URL, etc. regular expressions
        #region Email address
        /// <summary>
        /// Matches an e-mail address with required DNS domain name
        /// </summary>
        public const string RexEmailAddress = "(?i)^" + rexEmailId + "@(?<domainName>" + rexEmailIpAdr + @")$";

        readonly static Lazy<Regex> _emailAddress = new Lazy<Regex>(() => new Regex(RexEmailAddress, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches an e-mail address with required domain name
        /// </summary>
        public static Regex EmailAddress => _emailAddress.Value;

        /// <summary>
        /// Matches an e-mail address with required domain name
        /// </summary>
        public const string RexEmailIpAddress = "(?i)^" + rexEmailId + "@(?<domainName>" + rexEmailIpAdr + @"|\[" + rexEmailNumIp + @"\])$";

        readonly static Lazy<Regex> _emailIpAddress = new Lazy<Regex>(() => new Regex(RexEmailIpAddress, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches an e-mail address with required domain name
        /// </summary>
        public static Regex EmailIpAddress => _emailIpAddress.Value;
        #endregion

        #region URL full
        /// <summary>
        /// Matches URL root and path including intranet addresses (without domain and top level parts). 
        /// Note: in internet scenario these addresses may fail to connect to the resource, as they may be missing the top and domain level parts of the addresses, e.g. missing vm.com.
        /// </summary>
        public const string RexUrlFull = "(?i)^" + rexFullUrlRootAndPath + rexUrlQuery + "$";

        readonly static Lazy<Regex> _urlFull = new Lazy<Regex>(() => new Regex(RexUrlFull, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches URL root and path including intranet addresses (without domain and top level parts). 
        /// </summary>
        public static Regex UrlFull => _urlFull.Value;
        #endregion

        #region URL
        /// <summary>
        /// Matches URL root and path requiring FQDN, including domain and top level portions (vm.com), or IP address, or localhost.
        /// </summary>
        public const string RexUrl = "(?i)^" + rexUrlRootAndPath + rexUrlQuery + "$";

        readonly static Lazy<Regex> _rexUrl = new Lazy<Regex>(() => new Regex(RexUrl, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches URL root and path requiring FQDN, including domain and top level portions (vm.com), 
        /// or IP address, or localhost.
        /// </summary>
        public static Regex Url => _rexUrl.Value;
        #endregion

        #region WCF URL full
        /// <summary>
        /// Matches URL root and path including intranet addresses (without domain and top level parts). Allows for WCF schemes.
        /// </summary>
        public const string RexWcfUrlFull = "(?i)^" + rexFullWcfUrlRootAndPath + rexUrlQuery + "$";

        readonly static Lazy<Regex> _wcfUrlFull = new Lazy<Regex>(() => new Regex(RexWcfUrlFull, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches URL root and path including intranet addresses (without domain and top level parts).
        /// Allows for WCF schemes.
        /// </summary>
        public static Regex WcfUrlFull => _wcfUrlFull.Value;
        #endregion

        #region WCF URL
        /// <summary>
        /// Matches URL root and path requiring FQDN, i.e. all levels of DNS address, IP address or &quot;localhost&quot;. Allows for WCF schemes.
        /// </summary>
        public const string RexWcfUrl = "(?i)^" + rexWcfUrlRootAndPath + rexUrlQuery + "$";

        readonly static Lazy<Regex> _wcfUrl = new Lazy<Regex>(() => new Regex(RexWcfUrl, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches URL root and path requiring FQDN, i.e. all levels of DNS address, IP address or localhost. 
        /// Allows for WCF schemes.
        /// </summary>
        public static Regex WcfUrl => _wcfUrl.Value;
        #endregion

        #region WCF service with MSMQ transport
        /// <summary>
        /// Matches the address of a WCF service with an MSMQ transport. Allows for addresses without domain and top level parts.
        /// </summary>
        public const string RexWcfMsmqService = @"(?i)^(?<scheme>net\.msmq)://(?<host>" + rexUrlIpAdr + ")(?<scope>(/private|(/public)?))/(?<queue>.*)$";

        readonly static Lazy<Regex> _wcfMsmqService = new Lazy<Regex>(() => new Regex(RexWcfMsmqService, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches the address of a WCF service with an MSMQ transport.
        /// Allows for addresses without domain and top level parts.
        /// </summary>
        public static Regex WcfMsmqService => _wcfMsmqService.Value;
        #endregion
        #endregion

        #region Various codes and telephone numbers regular expressions
        #region Country ISO code - 2 letters
        /// <summary>
        /// Matches a two letter country code, e.g. US
        /// </summary>
        public const string RexCountryIsoCode2 = "(?i:^[a-z][a-z]$)";

        readonly static Lazy<Regex> _countryIsoCode2 = new Lazy<Regex>(() => new Regex(RexCountryIsoCode2, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches a two letter country code, e.g. US
        /// </summary>
        public static Regex CountryIsoCode2 => _countryIsoCode2.Value;
        #endregion

        #region Country ISO code - 3 letters
        /// <summary>
        /// Matches a three letter country code, e.g. USA
        /// </summary>
        public const string RexCountryIsoCode3 = "(?i:^[a-z][a-z][a-z]$)";

        readonly static Lazy<Regex> _countryIsoCode3 = new Lazy<Regex>(() => new Regex(RexCountryIsoCode3, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches a three letter country code, e.g. USA
        /// </summary>
        public static Regex CountryIsoCode3 => _countryIsoCode3.Value;
        #endregion

        #region Currency ISO code
        /// <summary>
        /// Matches a three letter Currency code, e.g. USD
        /// </summary>
        public const string RexCurrencyIsoCode = "(?i:^[a-z][a-z][a-z]$)";

        readonly static Lazy<Regex> _CurrencyIsoCode = new Lazy<Regex>(() => new Regex(RexCurrencyIsoCode, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches a three letter Currency code, e.g. USA
        /// </summary>
        public static Regex CurrencyIsoCode => _CurrencyIsoCode.Value;
        #endregion

        #region Country telephone code
        /// <summary>
        /// Matches a country telephone code
        /// </summary>
        public const string RexCountryCode = @"\d{1,3}";

        readonly static Lazy<Regex> _countryCode = new Lazy<Regex>(() => new Regex(RexCountryCode, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches a country telephone code, e.g. 1 or 359
        /// </summary>
        public static Regex CountryCode => _countryCode.Value;
        #endregion

        #region Telephone area code, e.g. 917
        /// <summary>
        /// Matches telephone area code
        /// </summary>
        public const string RexAreaCode = @"\d{1,5}";

        readonly static Lazy<Regex> _areaCode = new Lazy<Regex>(() => new Regex(RexAreaCode, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches telephone area code
        /// </summary>
        public static Regex AreaCode => _areaCode.Value;
        #endregion

        #region Telephone Number
        /// <summary>
        /// Matches a telephone number
        /// </summary>
        public const string RexTelephoneNumber = @"\d{4,15}";

        readonly static Lazy<Regex> _telephoneNumber = new Lazy<Regex>(() => new Regex(RexTelephoneNumber, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches a telephone number
        /// </summary>
        public static Regex TelephoneNumber => _telephoneNumber.Value;
        #endregion

        #region Social Security Number
        /// <summary>
        /// Matches a social security number, allowing any number of spaces, dashes and dots but requires 9 digits.
        /// </summary>
        public const string RexSocialSecurityNumber = @"^[-.\s]*(\d)[-.\s]*(\d)[-.\s]*(\d)[-.\s]*(\d)[-.\s]*(\d)[-.\s]*(\d)[-.\s]*(\d)[-.\s]*(\d)[-.\s]*(\d)[-.\s]*$";

        readonly static Lazy<Regex> _ssn = new Lazy<Regex>(() => new Regex(RexSocialSecurityNumber, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches a social security number, allowing any number of spaces, dashes and dots but requires 9 digits.
        /// </summary>
        public static Regex SocialSecurityNumber => _ssn.Value;
        #endregion

        /// <summary>
        /// Formats the result of the above successful match to the common format ddd-dd-dddd
        /// </summary>
        public const string ReplaceSocialSecurityNumber = "$1$2$3-$4$5-$6$7$8$9";

        #region InvalidSocialSecurityNumber
        /// <summary>
        /// Tests the commonly formatted social security number for invalidity, including some notorious SSN-s
        /// </summary>
        public const string RexInvalidSocialSecurityNumber = @"000-\d\d-\d\d\d\d|" +
                                                             @"\d\d\d-00-\d\d\d\d|" +
                                                             @"\d\d\d-\d\d-0000|" +
                                                             @"666-\d\d-\d\d\d\d|" +
                                                             @"[89]\d\d-\d\d-\d\d\d\d|" +
                                                             @"7[89]\d-\d\d-\d\d\d\d|" +
                                                             @"77[3-9]-\d\d-\d\d\d\d|" +
                                                             // well known invalid SSN-s:
                                                             @"001-01-0001|" +
                                                             @"078-05-1120|" +
                                                             @"433-54-3937";

        readonly static Lazy<Regex> _invalidSsn = new Lazy<Regex>(() => new Regex(RexInvalidSocialSecurityNumber, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which tests the commonly formatted social security number for invalidity, including some notorious SSN-s
        /// </summary>
        public static Regex InvalidSocialSecurityNumber => _invalidSsn.Value;
        #endregion

        #region IndividualTaxpayerIdentification
        /// <summary>
        /// Tests the commonly formatted social security number for Individual Taxpayer Identification Number (ITIN) - issued to foreigners.
        /// </summary>
        public const string RexIndividualTaxpayerIdentificationNumber = @"9\d\d-[7-9]\d-\d\d\d\d|";

        readonly static Lazy<Regex> _itin = new Lazy<Regex>(() => new Regex(RexIndividualTaxpayerIdentificationNumber, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which tests the commonly formatted social security number for for Individual Taxpayer Identification Number (ITIN)
        /// </summary>
        public static Regex IndividualTaxpayerIdentificationNumber => _itin.Value;
        #endregion
        #endregion

        #region Bank identification codes regular expressions
        #region AbaRoutingNumber
        /// <summary>
        /// Matches an ABA routing number.
        /// </summary>
        public const string RexAbaRoutingNumber = @"^\d{9}$";

        readonly static Lazy<Regex> _routingNumber = new Lazy<Regex>(() => new Regex(RexAbaRoutingNumber, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches ...
        /// </summary>
        public static Regex AbaRoutingNumber => _routingNumber.Value;
        #endregion

        #region SwiftBankCode
        /// <summary>
        /// Matches SWIFT BIC code.
        /// </summary>
        public const string RexSwiftBankCode = @"(?i:^[a-z]{4}[a-z]{2}[a-z\d]{2}(?:[a-z\d]{3})?$)";

        readonly static Lazy<Regex> _swiftBankCode = new Lazy<Regex>(() => new Regex(RexSwiftBankCode, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches SWIFT BIC code.
        /// </summary>
        public static Regex SwiftBankCode => _swiftBankCode.Value;
        #endregion
        #endregion

        #region Semantic version
        /// <summary>
        /// The semantic versioning prerelease version syntax.
        /// </summary>
        public const string RexSemanticVersionPrerelease = @"[0-9a-z-]+(?:\.[0-9a-z-]+)*";

        readonly static Lazy<Regex> _semanticVersionPrerelease = new Lazy<Regex>(() => new Regex(RexSemanticVersionPrerelease, RegexOptions.Compiled));

        /// <summary>
        /// Gets a regular expression object that tests semantic versioning prerelease strings.
        /// </summary>
        public static Regex SemanticVersionPrerelease => _semanticVersionPrerelease.Value;

        /// <summary>
        /// The semantic versioning prerelease version syntax.
        /// </summary>
        public const string RexSemanticVersionBuild = RexSemanticVersionPrerelease;

        /// <summary>
        /// Gets a regular expression object that tests semantic versioning build strings.
        /// </summary>
        public static Regex SemanticVersionBuild => _semanticVersionPrerelease.Value;

        /// <summary>
        /// Tests semantic versioning strings: http://semver.org/
        /// </summary>
        public const string RexSemanticVersion = @"^(?i:)(?<major>0|(?:0|[1-9][0-9]*))\.(?<minor>0|[1-9][0-9]*)\.(?<patch>0|[1-9][0-9]*)(?:-(?<prerelease>"+RexSemanticVersionPrerelease+@"))?(?:\+(?<build>"+RexSemanticVersionBuild+@"))?$";

        readonly static Lazy<Regex> _semanticVersion = new Lazy<Regex>(() => new Regex(RexSemanticVersion, RegexOptions.Compiled));

        /// <summary>
        /// Gets a regular expression object that tests semantic versioning strings.
        /// </summary>
        public static Regex SemanticVersion => _semanticVersion.Value;
        #endregion

        #region Guid
        const string rexGuidWithDashes = @"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}";
        const string rexGuidWithNoDashes = @"[0-9a-f]{32}";
        const string rexGuid = @"\{?(?:"+rexGuidWithDashes+@")|(?:"+rexGuidWithNoDashes+@")\}?";

        #region GUID somewhere in the input string
        /// <summary>
        /// Regular expression pattern which matches GUID somewhere in the input string.
        /// </summary>
        public const string RexGuid = @"(?i:)"+rexGuid;

        readonly static Lazy<Regex> _rexGuid = new Lazy<Regex>(() => new Regex(RexGuid, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches GUID
        /// </summary>
        public static Regex Guid => _rexGuid.Value;
        #endregion

        #region Matches exactly GUID against the input string
        /// <summary>
        /// Regular expression pattern which matches exactly GUID against the input string.
        /// </summary>
        public const string RexExactGuid = @"(?i:)^"+rexGuid+@"$";

        readonly static Lazy<Regex> _rexExactGuid = new Lazy<Regex>(() => new Regex(RexExactGuid, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches GUID
        /// </summary>
        public static Regex ExactGuid => _rexExactGuid.Value;
        #endregion
        #endregion

        #region Content-type or Accepts header values:
        /// <summary>
        /// Regular expression pattern which matches the value of the HTTP headers Accept and content-type, incl. vendor specific MIME types.
        /// </summary>
        public const string RexContentType = @"(?i:)^(?<type>text|application)/(?:(?<vendor>[^\s,;+-]+)(?:-(?<version>[^\s,;+]+))?\+)?(?<format>[^\s\+,;]+)$";

        readonly static Lazy<Regex> _contentType = new Lazy<Regex>(() => new Regex(RexContentType, RegexOptions.Compiled));

        /// <summary>
        /// Regular expression pattern which matches the value of the HTTP headers Accept and content-type, incl. vendor specific MIME types.
        /// </summary>
        public static Regex ContentType => _contentType.Value;
        #endregion

        #region ByteArray
        /// <summary>
        /// Matches a text representation of a byte array the way it is produces by BitConverter,
        /// e.g. 01-23-45-67-89-ab-cd-ef
        /// </summary>
        public const string RexByteArray = "(?i:^[0-9a-f][0-9a-f](-[0-9a-f][0-9a-f])*$)";

        readonly static Lazy<Regex> _byteArray = new Lazy<Regex>(() => new Regex(RexByteArray, RegexOptions.Compiled));

        /// <summary>
        /// Gets a Regex object which matches a text representation of a byte array the way it is produces by BitConverter,
        /// e.g. 01-23-45-67-89-ab-cd-ef
        /// </summary>
        public static Regex ByteArray => _byteArray.Value;
        #endregion

        #region ConnectionString
        /// <summary>
        /// Matches a connection string. For a list of the formatting rules see http://www.connectionstrings.com/formating-rules-for-connection-strings/.
        /// e.g. &quot;Server=(localdb)\MSSQLLocalDB;Database=Northwind;Integrated Security=true;MultipleActiveResultSets=True;Asynchronous Processing=True;Application Name=IQToolkit;&quot;
        /// </summary>
        public const string RexConnectionString = @"(?<key>(?:[^""'=;\s]|==)*(?:[^""'=;\s]|==))\s*=\s*(?:(?<value>[^;'""{\s][^;]*[^;\s])|(?:""(?<value>[^""]+)"")|(?:'(?<value>[^']+)')|(?:\{(?<value>[^\}]+)\}))";

        readonly static Lazy<Regex> _connectionString = new Lazy<Regex>(() => new Regex(RexConnectionString, RegexOptions.Compiled));

        /// <summary>
        /// Matches a connection string. For a list of the formatting rules see http://www.connectionstrings.com/formating-rules-for-connection-strings/.
        /// e.g. &quot;Server=(localdb)\MSSQLLocalDB;Database=Northwind;Integrated Security=true;MultipleActiveResultSets=True;Asynchronous Processing=True;Application Name=IQToolkit;&quot;
        /// </summary>
        public static Regex ConnectionString => _connectionString.Value;
        #endregion

        #region C# identifier
        /// <summary>
        /// Matches a C# identifier.
        /// </summary>
        public const string RexCSharpIdentifier = @"^[_\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Mc}\p{Cf}\p{Pc}\p{Lm}][_\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]*$";

        readonly static Lazy<Regex> _cSharpIdentifier = new Lazy<Regex>(() => new Regex(RexCSharpIdentifier, RegexOptions.Compiled));

        /// <summary>
        /// Matches a C# identifier.
        /// </summary>
        public static Regex CSharpIdentifier => _cSharpIdentifier.Value;
        #endregion

        #region ISO 8601 date and time strings:
        /// <summary>
        /// Matches date and time value expressed in ISO 8601 standard format:
        /// </summary>
        public const string RexDateTimeIso8601 = @"((((\d{4})(-((0[1-9])|(1[012])))(-((0[1-9])|([12]\d)|(3[01]))))(T((([01]\d)|(2[0123]))((:([012345]\d))((:([012345]\d))(\.(\d+))?)?)?)(Z|([\+\-](([01]\d)|(2[0123]))(:([012345]\d))?)))?)|(((\d{4})((0[1-9])|(1[012]))((0[1-9])|([12]\d)|(3[01])))(T((([01]\d)|(2[0123]))(([012345]\d)(([012345]\d)(\d+)?)?)?)(Z|([\+\-](([01]\d)|(2[0123]))([012345]\d)?)))?))";

        readonly static Lazy<Regex> _dateTimeIso8601 = new Lazy<Regex>(() => new Regex(RexDateTimeIso8601, RegexOptions.Compiled));

        /// <summary>
        /// Matches date and time value expressed in ISO 8601 standard format:
        /// </summary>
        public static Regex DateTimeIso8601 => _dateTimeIso8601.Value;
        #endregion

        /// <summary>
        /// Dumps all regular expressions in this class.
        /// </summary>
        [Conditional("DEBUG")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object,System.Object)")]
        public static void DumpAllRegularExpressions()
        {
            typeof(RegularExpression)
                .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(fi => fi.Name.StartsWith("rex", StringComparison.OrdinalIgnoreCase))
                .OrderBy(fi => fi.Name, StringComparer.Ordinal)
                .Select(fi => { Console.WriteLine("{0} : {1}", fi.Name, fi.GetValue(null)); return 1; })
                .Count();
        }
    }
}
