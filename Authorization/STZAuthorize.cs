namespace STZ.Frontend.Authorization;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class STZAuthorize : Attribute
{
    public string Feature { get; }
    public string Action { get; set; } = "View";
    
    public STZAuthorize(string feature)
    {
        Feature = feature;
    }
}