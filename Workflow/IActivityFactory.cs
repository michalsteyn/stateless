namespace Stateless.Workflow
{
    public interface IActivityFactory
    {
        TActivity GetActivity<TActivity>();
    }
}