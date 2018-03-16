using System;

namespace WorkflowExample
{    
    //Used for DI of activities into Workflow 
    public interface IActivityFactory
    {
        TActivity GetActivity<TActivity>();
    }

    public class ActivityFactory : IActivityFactory
    {
        public TActivity GetActivity<TActivity>()
        {
            return Activator.CreateInstance<TActivity>();
        }
    }
}