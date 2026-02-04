namespace NirvanaPublic.Entities.NEL;

public class EntityProxyItem {
    public required string NickName { get; set; }
    public required int LocalPort { get; set; }
    public required string ForwardAddress { get; set; }
    public required int ForwardPort { get; set; }
    public required string ServerName { get; set; }
    public required string ServerVersion { get; set; }
}