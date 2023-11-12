namespace Bitbucket.Models.Interfaces
{
    public interface IResponse
    {
        object Value { get; set; }
        long Time { get; set; }
    }
}
