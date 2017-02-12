using UnityEngine;
using System.Collections;

namespace NodeCanvas.Framework
{
    public interface IUpdatable
    {
        void Update();
    }

    public interface ISubParametersContainer
    {
        BBParameter[] GetIncludeParseParameters();
    }
    public interface ITaskAssignable
    {
        Task task { get; set; }
    }

    public interface ISubTasksContainer
    {
        Task[] GetTasks();
    }

}

