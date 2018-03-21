using Autofac;
using Stateless.Workflow;

namespace WorkflowExample
{    
    //Used for DI of activities into Workflow 

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