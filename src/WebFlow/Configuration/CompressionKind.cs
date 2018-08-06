using System.Xml.Serialization;

namespace Acklann.WebFlow.Configuration
{
    public enum CompressionKind
    {
        [XmlEnum("lossless")]
        LossLess,

        [XmlEnum("lossy")]
        Lossy
    }
}