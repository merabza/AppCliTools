//using System.Collections.Generic;

//namespace CliParameters.Tasks
//{
//    public sealed class TasksCollection
//    {

//        public Dictionary<string, TaskModel> Tasks { get; set; }

//        public TaskModel GetTask(string taskName)
//        {
//            if (taskName == null || Tasks == null || !Tasks.ContainsKey(taskName))
//                return null;
//            return Tasks[taskName];
//        }

//        public bool CheckNewTaskNameValid(string oldTaskName, string newTaskName)
//        {

//            if (oldTaskName == newTaskName)
//                return true;

//            if (!Tasks.ContainsKey(oldTaskName))
//                return false;

//            return !Tasks.ContainsKey(newTaskName);
//        }

//        public bool RemoveTask(string taskName)
//        {
//            if (Tasks == null || !Tasks.ContainsKey(taskName))
//                return false;
//            Tasks.Remove(taskName);
//            return true;
//        }

//        public bool AddTask(string newTaskName, TaskModel task)
//        {
//            if (Tasks != null && Tasks.ContainsKey(newTaskName))
//                return false;
//            Tasks ??= new Dictionary<string, TaskModel>();
//            Tasks.Add(newTaskName, task);
//            return true;
//        }

//    }
//}

