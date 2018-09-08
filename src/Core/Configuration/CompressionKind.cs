using System.Xml.Serialization;

namespace Acklann.Traneleon.Configuration
{
    public enum CompressionKind
    {
        [XmlEnum("lossless")]
        LossLess,

        [XmlEnum("lossy")]
        Lossy
    }
}