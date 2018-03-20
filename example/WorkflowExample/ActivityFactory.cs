using Autofac;

namespace WorkflowExample
{    
    //Used for DI of activities into Workflow 
    public interface IActivityFactory
    {
        TActivity GetActivity<TActivity>();
    }

    public class ActivityFactory : IActivityFactory
    {
        private readonly ILifetimeScope _container;

        public ActivityFactory(ILifetimeScope container)
        {
            _container = container;
        }

        public TActivity GetActivity<TActivity>()
        {
            return _container.Resolve<TActivity>();
        }
    }
}