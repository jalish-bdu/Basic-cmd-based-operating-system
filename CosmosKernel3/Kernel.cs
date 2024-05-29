using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sys = Cosmos.System;

namespace HorizonOs
{
    public class Kernel : Sys.Kernel

    {
        private string currentDirectory = "/";
        private IFileSystem fileSystem;

        private Queue<Process> readyQueue = new Queue<Process>();
        private Process currentProcess = null;

        private const int MemorySize = 2048;  // Fixed memory size
        private List<MemoryBlock> memoryBlocks = new List<MemoryBlock>();

        private int[] buffer;
        private int size;
        private int start;
        private int end;
        private int count;
        private object lockObj = new object();

        protected override void BeforeRun()
        {
            Console.WriteLine("\n");
            Console.WriteLine("--------++++++++++****\tBasic CMD Based Operating System\t++++++++++****--------");
            Console.WriteLine("\n");
            Console.WriteLine("Operating System booted successfully. Go ahead.");
            Console.WriteLine("Input 'cmd' to see available command list");

            fileSystem = new InMemoryFileSystem();
            try
            {
                fileSystem.CreateDirectory("/");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing root directory: " + ex.Message);
            }

            memoryBlocks.Add(new MemoryBlock(1, MemorySize)); // Initialize with one large free block
        }

        protected override void Run()
        {
            while (true)
            {
                PrintShellPrompt();

                string cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "cmd":
                        PrintCommands();
                        break;
                    case "echo":
                        EchoCommand();
                        break;
                    case "date":
                        Console.WriteLine("Date : " + DateTime.Today.ToShortDateString());
                        Console.Write("\n");
                        break;
                    case "time":
                        Console.WriteLine("Time : " + DateTime.Now.ToShortTimeString());
                        Console.Write("\n");
                        break;
                    case "shutdown":
                        Sys.Power.Shutdown();
                        Console.Write("\n");
                        break;
                    case "createfile":
                        CreateFileCommand();
                        break;
                    case "readfile":
                        ReadFileCommand();
                        break;
                    case "appendfile":
                        AppendFileCommand();
                        break;
                    case "updatefile":
                        UpdateFileCommand();
                        break;
                    case "deletefile":
                        DeleteFileCommand();
                        break;
                    case "createdir":
                        CreateDirectoryCommand();
                        break;
                    case "deletedir":
                        DeleteDirectoryCommand();
                        break;
                    case "cd":
                        ChangeDirectoryCommand();
                        break;
                    case "ls":
                        ListDirectoryContents();
                        break;
                    case "cpuscd":
                        PerformCpuScheduling();
                        break;
                    case "mem":
                        PerformMemoryManagement();
                        break;
                    case "ipc":
                        Console.WriteLine("Due to infinity loop, we commented out the code");
                        StartIPC();
                        break;
                    default:
                        Console.WriteLine("Invalid command");
                        break;
                }
            }
        }

        private void PrintShellPrompt()
        {
            Console.Write($"{currentDirectory} $ ");
        }

        private void PrintCommands()
        {
            Console.WriteLine("1. To print anything: echo");
            Console.WriteLine("2. To show date: date");
            Console.WriteLine("3. To show time: time");
            Console.WriteLine("4. To create a file: createfile");
            Console.WriteLine("5. To read from the file: readfile");
            Console.WriteLine("6. To append in the file: appendfile");
            Console.WriteLine("7. To update the file: updatefile");
            Console.WriteLine("8. To delete the file: deletefile");
            Console.WriteLine("9. To create directory: createdir");
            Console.WriteLine("10. To delete directory: deletedir");
            Console.WriteLine("11. To show list: ls");
            Console.WriteLine("12. To change directory: cd");
            Console.WriteLine("13. To perform cpu scheduling: cpuscd");
            Console.WriteLine("14. To perform interprocess communication: ipc");
            Console.WriteLine("15. To perform memory management: mem");
            Console.WriteLine("16. To shutdown the OS: shutdown");
            Console.Write("\n");
        }

        private void EchoCommand()
        {
            Console.Write("Input your text: ");
            var input = Console.ReadLine();
            Console.Write("Text typed: ");
            Console.WriteLine(input);
            Console.Write("\n");
        }

        private void CreateFileCommand()
        {
            try
            {
                Console.WriteLine("Enter file name: ");
                string fileName = Console.ReadLine();
                Console.WriteLine("Enter file content: ");
                string fileContent = Console.ReadLine();
                fileSystem.CreateFile(GetFullPath(fileName), fileContent);
                Console.WriteLine("File created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating file: " + ex.Message);
            }
        }

        private void ReadFileCommand()
        {
            try
            {
                Console.WriteLine("Enter file name: ");
                string fileToRead = Console.ReadLine();
                Console.WriteLine(fileSystem.ReadFile(GetFullPath(fileToRead)));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading file: " + ex.Message);
            }
        }

        private void AppendFileCommand()
        {
            try
            {
                Console.WriteLine("Enter file name: ");
                string fileToAppend = Console.ReadLine();
                Console.WriteLine("Enter content to append: ");
                string contentToAppend = Console.ReadLine();
                fileSystem.AppendToFile(GetFullPath(fileToAppend), contentToAppend);
                Console.WriteLine("Content appended successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error appending to file: " + ex.Message);
            }
        }

        private void UpdateFileCommand()
        {
            try
            {
                Console.WriteLine("Enter file name: ");
                string fileToUpdate = Console.ReadLine();
                Console.WriteLine("Enter new content: ");
                string newContent = Console.ReadLine();
                fileSystem.UpdateFile(GetFullPath(fileToUpdate), newContent);
                Console.WriteLine("File updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating file: " + ex.Message);
            }
        }

        private void DeleteFileCommand()
        {
            try
            {
                Console.WriteLine("Enter file name to delete: ");
                string fileToDelete = Console.ReadLine();
                fileSystem.DeleteFile(GetFullPath(fileToDelete));
                Console.WriteLine("File deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting file: " + ex.Message);
            }
        }

        private void CreateDirectoryCommand()
        {
            try
            {
                Console.WriteLine("Enter directory name to create: ");
                string directoryToCreate = Console.ReadLine();
                fileSystem.CreateDirectory(GetFullPath(directoryToCreate));
                Console.WriteLine("Directory created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating directory: " + ex.Message);
            }
        }

        private void DeleteDirectoryCommand()
        {
            try
            {
                Console.WriteLine("Enter directory name to delete: ");
                string directoryToDelete = Console.ReadLine();
                fileSystem.DeleteDirectory(GetFullPath(directoryToDelete));
                Console.WriteLine("Directory deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting directory: " + ex.Message);
            }
        }

        private void ChangeDirectoryCommand()
        {
            try
            {
                Console.WriteLine("Enter directory path: ");
                string newDirectory = Console.ReadLine();
                ChangeDirectory(newDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error changing directory: " + ex.Message);
            }
        }

        private void ChangeDirectory(string newDirectory)
        {
            try
            {
                if (newDirectory == "..")
                {
                    if (currentDirectory != "/")
                    {
                        int lastSlash = currentDirectory.LastIndexOf('/');
                        currentDirectory = currentDirectory.Substring(0, lastSlash);
                        if (string.IsNullOrEmpty(currentDirectory))
                        {
                            currentDirectory = "/";
                        }
                    }
                }
                else
                {
                    string fullPath = GetFullPath(newDirectory);
                    if (fileSystem.DirectoryExists(fullPath))
                    {
                        currentDirectory = fullPath;
                    }
                    else
                    {
                        Console.WriteLine("Directory not found: " + newDirectory);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error changing directory: " + ex.Message);
            }
        }

        private void ListDirectoryContents()
        {
            try
            {
                Console.WriteLine("Directory contents:");
                foreach (var item in fileSystem.ListDirectory(GetFullPath(currentDirectory)))
                {
                    Console.WriteLine(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error listing directory contents: " + ex.Message);
            }
        }

        private string GetFullPath(string path)
        {
            if (path.StartsWith("/"))
            {
                return path;
            }
            if (currentDirectory == "/")
            {
                return "/" + path;
            }
            return currentDirectory + "/" + path;
        }

        private void StartIPC()
        {
            try
            {
                InitializeBuffer(10);

                // Simulate production and consumption
                for (int i = 0; i < 10; i++)
                {
                    Produce(i);
                }

                for (int i = 0; i < 10; i++)
                {
                    Consume();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartIPC: {ex.Message}");
            }

            // Halt to avoid reboot loop
            Console.WriteLine("IPC example completed. System will halt.");
            Sys.Power.Shutdown();
        }

        private void InitializeBuffer(int bufferSize)
        {
            buffer = new int[bufferSize];
            size = bufferSize;
            start = 0;
            end = 0;
            count = 0;
        }

        private bool IsEmpty()
        {
            return count == 0;
        }

        private bool IsFull()
        {
            return count == size;
        }

        private void Enqueue(int item)
        {
            if (IsFull())
            {
                throw new InvalidOperationException("Buffer is full");
            }

            buffer[end] = item;
            end = (end + 1) % size;
            count++;
        }

        private int Dequeue()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("Buffer is empty");
            }

            int item = buffer[start];
            start = (start + 1) % size;
            count--;
            return item;
        }

        private void Produce(int item)
        {
            lock (lockObj)
            {
                while (IsFull())
                {
                    // Wait until there is space in the buffer
                }

                Enqueue(item);
                Console.WriteLine($"Produced: {item}");
            }
        }

        private void Consume()
        {
            lock (lockObj)
            {
                while (IsEmpty())
                {
                    // Wait until there is something in the buffer
                }

                int item = Dequeue();
                Console.WriteLine($"Consumed: {item}");
            }
        }
        private void PerformCpuScheduling()
        {
            Console.Write("\nCPU SCHEDULING....\n");
            Console.Write("Enter the number of processes: ");
            int n = int.Parse(Console.ReadLine());

            int[] burstTime = new int[n];
            int[] arrivalTime = new int[n];
            int[] priority = new int[n];

            // Input process details
            for (int i = 0; i < n; i++)
            {
                Console.Write($"Enter burst time for process {i + 1}: ");
                burstTime[i] = int.Parse(Console.ReadLine());
                Console.Write($"Enter arrival time for process {i + 1}: ");
                arrivalTime[i] = int.Parse(Console.ReadLine());
                Console.Write($"Enter priority for process {i + 1}: ");
                priority[i] = int.Parse(Console.ReadLine());
            }

            // Menu-driven loop for scheduling algorithms
            while (true)
            {
                Console.WriteLine("\nScheduling Algorithms Menu:");
                Console.WriteLine("1. FCFS");
                Console.WriteLine("2. SJF");
                Console.WriteLine("3. Round Robin");
                Console.WriteLine("4. Priority");
                Console.WriteLine("5. Return to main menu");
                Console.Write("Enter your choice: ");
                int choice = int.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        Fcfs(n, burstTime, arrivalTime, priority);
                        break;
                    case 2:
                        Sjf(n, burstTime, arrivalTime);
                        break;
                    case 3:
                        Console.Write("Enter time quantum for Round Robin: ");
                        int quantum = int.Parse(Console.ReadLine());
                        RoundRobin(n, burstTime, arrivalTime, quantum);
                        break;
                    case 4:
                        PriorityS(n, burstTime, arrivalTime, priority);
                        break;
                    case 5:
                        return;
                    default:
                        Console.WriteLine("Invalid choice! Please enter a number between 1 and 5.");
                        break;
                }
            }
        }

        // First Come First Serve (FCFS) Scheduling Algorithm
        private void Fcfs(int n, int[] burstTime, int[] arrivalTime, int[] priority)
        {
            int[] waitTime = new int[n];
            int[] turnAroundTime = new int[n];
            int totalWaitTime = 0, totalTurnAroundTime = 0;
            int currentTime = 0;
            for (int i = 0; i < n; i++)
            {
                if (currentTime < arrivalTime[i])
                {
                    currentTime = arrivalTime[i];
                }
                waitTime[i] = currentTime - arrivalTime[i];
                turnAroundTime[i] = waitTime[i] + burstTime[i];
                currentTime += burstTime[i];
                totalWaitTime += waitTime[i];
                totalTurnAroundTime += turnAroundTime[i];

                // Display results
                Console.WriteLine($"Process {i + 1}:");
                Console.WriteLine($"Waiting Time: {waitTime[i]}");
                Console.WriteLine($"Turnaround Time: {turnAroundTime[i]}");
            }

            Console.WriteLine($"Average Waiting Time: {(float)totalWaitTime / n:F2}");
            Console.WriteLine($"Average Turnaround Time: {(float)totalTurnAroundTime / n:F2}");
        }

        // Shortest Job First (SJF) Scheduling Algorithm
        private void Sjf(int n, int[] burstTime, int[] arrivalTime)
        {
            int[] waitTime = new int[n];
            int[] turnAroundTime = new int[n];
            int totalWaitTime = 0, totalTurnAroundTime = 0;
            int completed = 0, currentTime = 0;
            bool[] isCompleted = new bool[n];

            while (completed != n)
            {
                int idx = -1;
                int minBurst = int.MaxValue;

                for (int i = 0; i < n; i++)
                {
                    if (arrivalTime[i] <= currentTime && !isCompleted[i] && burstTime[i] < minBurst)
                    {
                        minBurst = burstTime[i];
                        idx = i;
                    }
                }

                if (idx != -1)
                {
                    waitTime[idx] = currentTime - arrivalTime[idx];
                    turnAroundTime[idx] = waitTime[idx] + burstTime[idx];
                    currentTime += burstTime[idx];
                    isCompleted[idx] = true;
                    completed++;

                    // Display results
                    Console.WriteLine($"Process {idx + 1}:");
                    Console.WriteLine($"Waiting Time: {waitTime[idx]}");
                    Console.WriteLine($"Turnaround Time: {turnAroundTime[idx]}");

                    totalWaitTime += waitTime[idx];
                    totalTurnAroundTime += turnAroundTime[idx];
                }
                else
                {
                    currentTime++;
                }
            }

            Console.WriteLine($"Average Waiting Time: {(float)totalWaitTime / n:F2}");
            Console.WriteLine($"Average Turnaround Time: {(float)totalTurnAroundTime / n:F2}");
        }

        // Round Robin Scheduling Algorithm
        private void RoundRobin(int n, int[] burstTime, int[] arrivalTime, int quantum)
        {
            int[] remainingTime = new int[n];
            int[] waitTime = new int[n];
            int[] turnAroundTime = new int[n];
            int totalWaitTime = 0, totalTurnAroundTime = 0;
            int currentTime = 0;
            int completed = 0;

            for (int i = 0; i < n; i++)
            {
                remainingTime[i] = burstTime[i];
            }

            while (completed != n)
            {
                for (int i = 0; i < n; i++)
                {
                    if (arrivalTime[i] <= currentTime && remainingTime[i] > 0)
                    {
                        if (remainingTime[i] <= quantum)
                        {
                            currentTime += remainingTime[i];
                            remainingTime[i] = 0;
                            completed++;
                            waitTime[i] = currentTime - arrivalTime[i] - burstTime[i];
                            turnAroundTime[i] = currentTime - arrivalTime[i];
                            // Display results
                            Console.WriteLine($"Process {i + 1}:");
                            Console.WriteLine($"Waiting Time: {waitTime[i]}");
                            Console.WriteLine($"Turnaround Time: {turnAroundTime[i]}");

                            totalWaitTime += waitTime[i];
                            totalTurnAroundTime += turnAroundTime[i];
                        }
                        else
                        {
                            remainingTime[i] -= quantum;
                            currentTime += quantum;
                        }
                    }
                }
            }

            Console.WriteLine($"Average Waiting Time: {(float)totalWaitTime / n:F2}");
            Console.WriteLine($"Average Turnaround Time: {(float)totalTurnAroundTime / n:F2}");
        }

        // Priority Scheduling Algorithm
        private void PriorityS(int n, int[] burstTime, int[] arrivalTime, int[] priority)
        {
            int[] waitTime = new int[n];
            int[] turnAroundTime = new int[n];
            int totalWaitTime = 0, totalTurnAroundTime = 0;
            int completed = 0, currentTime = 0;
            bool[] isCompleted = new bool[n];

            while (completed != n)
            {
                int idx = -1;
                int highestPriority = int.MaxValue;

                for (int i = 0; i < n; i++)
                {
                    if (arrivalTime[i] <= currentTime && !isCompleted[i] && priority[i] < highestPriority)
                    {
                        highestPriority = priority[i];
                        idx = i;
                    }
                }

                if (idx != -1)
                {
                    waitTime[idx] = currentTime - arrivalTime[idx];
                    turnAroundTime[idx] = waitTime[idx] + burstTime[idx];
                    currentTime += burstTime[idx];
                    isCompleted[idx] = true;
                    completed++;

                    // Display results
                    Console.WriteLine($"Process {idx + 1}:");
                    Console.WriteLine($"Waiting Time: {waitTime[idx]}");
                    Console.WriteLine($"Turnaround Time: {turnAroundTime[idx]}");

                    totalWaitTime += waitTime[idx];
                    totalTurnAroundTime += turnAroundTime[idx];
                }
                else
                {
                    currentTime++;
                }
            }

            Console.WriteLine($"Average Waiting Time: {(float)totalWaitTime / n:F2}");
            Console.WriteLine($"Average Turnaround Time: {(float)totalTurnAroundTime / n:F2}");
        }

        private void PerformMemoryManagement()
        {
            Console.Write("\nMEMORY MANAGEMENT.....\n");
            Console.Write("Enter the number of memory blocks: ");
            int blocks = int.Parse(Console.ReadLine());

            int[] blockSize = new int[blocks];
            Console.WriteLine("Enter the size of each memory block:");
            for (int i = 0; i < blocks; i++)
            {
                blockSize[i] = int.Parse(Console.ReadLine());
            }

            Console.Write("Enter the number of processes: ");
            int processes = int.Parse(Console.ReadLine());

            int[] processSize = new int[processes];
            Console.WriteLine("Enter the size of each process:");
            for (int i = 0; i < processes; i++)
            {
                processSize[i] = int.Parse(Console.ReadLine());
            }

            while (true)
            {
                Console.WriteLine("\n1. First Fit\n2. Best Fit\n3. Worst Fit\n4. Return to main menu\n");
                Console.Write("Enter your choice: ");
                int choice = int.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        ImplementFirstFit(blockSize, blocks, processSize, processes);
                        break;
                    case 2:
                        ImplementBestFit(blockSize, blocks, processSize, processes);
                        break;
                    case 3:
                        ImplementWorstFit(blockSize, blocks, processSize, processes);
                        break;
                    case 4:
                        return;
                    default:
                        Console.WriteLine("Invalid choice!");
                        break;
                }
            }
        }

        public static void ImplementFirstFit(int[] blockSize, int blocks, int[] processSize, int processes)
        {
            int[] allocation = new int[processes];
            bool[] occupied = new bool[blocks];

            for (int i = 0; i < processes; i++)
            {
                allocation[i] = -1;
            }

            for (int i = 0; i < blocks; i++)
            {
                occupied[i] = false;
            }

            for (int i = 0; i < processes; i++)
            {
                for (int j = 0; j < blocks; j++)
                {
                    if (!occupied[j] && blockSize[j] >= processSize[i])
                    {
                        allocation[i] = j;
                        occupied[j] = true;
                        break;
                    }
                }
            }

            Console.WriteLine("\nProcess No.\tProcess Size\tBlock no.\tBlock Size");
            for (int i = 0; i < processes; i++)
            {
                Console.Write($"{i + 1}\t\t{processSize[i]}\t\t");
                if (allocation[i] != -1)
                    Console.WriteLine($"{allocation[i] + 1}\t\t{blockSize[allocation[i]]}");
                else
                    Console.WriteLine("Not Allocated");
            }
        }

        public static void ImplementBestFit(int[] blockSize, int blocks, int[] processSize, int processes)
        {
            int[] allocation = new int[processes];
            bool[] occupied = new bool[blocks];

            for (int i = 0; i < processes; i++)
            {
                allocation[i] = -1;
            }

            for (int i = 0; i < blocks; i++)
            {
                occupied[i] = false;
            }

            for (int i = 0; i < processes; i++)
            {
                int indexPlaced = -1;
                for (int j = 0; j < blocks; j++)
                {
                    if (blockSize[j] >= processSize[i] && !occupied[j])
                    {
                        if (indexPlaced == -1)
                            indexPlaced = j;
                        else if (blockSize[j] < blockSize[indexPlaced])
                            indexPlaced = j;
                    }
                }

                if (indexPlaced != -1)
                {
                    allocation[i] = indexPlaced;
                    occupied[indexPlaced] = true;
                }
            }

            Console.WriteLine("\nProcess No.\tProcess Size\tBlock no.\tBlock Size");
            for (int i = 0; i < processes; i++)
            {
                Console.Write($"{i + 1}\t\t{processSize[i]}\t\t");
                if (allocation[i] != -1)
                    Console.WriteLine($"{allocation[i] + 1}\t\t{blockSize[allocation[i]]}");
                else
                    Console.WriteLine("Not Allocated");
            }
        }

        public static void ImplementWorstFit(int[] blockSize, int blocks, int[] processSize, int processes)
        {
            int[] allocation = new int[processes];
            bool[] occupied = new bool[blocks];

            for (int i = 0; i < processes; i++)
            {
                allocation[i] = -1;
            }

            for (int i = 0; i < blocks; i++)
            {
                occupied[i] = false;
            }

            for (int i = 0; i < processes; i++)
            {
                int indexPlaced = -1;
                for (int j = 0; j < blocks; j++)
                {
                    if (blockSize[j] >= processSize[i] && !occupied[j])
                    {
                        if (indexPlaced == -1)
                            indexPlaced = j;
                        else if (blockSize[indexPlaced] < blockSize[j])
                            indexPlaced = j;
                    }
                }

                if (indexPlaced != -1)
                {
                    allocation[i] = indexPlaced;
                    occupied[indexPlaced] = true;
                    blockSize[indexPlaced] -= processSize[i];
                }
            }

            Console.WriteLine("\nProcess No.\tProcess Size\tBlock no.\tWastage Memory");
            for (int i = 0; i < processes; i++)
            {
                Console.Write($"{i + 1}\t\t{processSize[i]}\t\t");
                if (allocation[i] != -1)
                    Console.WriteLine($"{allocation[i] + 1}\t\t{blockSize[allocation[i]]}");
                else
                    Console.WriteLine("Not Allocated");
            }
        }



        public interface IFileSystem
        {
            void CreateFile(string path, string content);
            string ReadFile(string path);
            void AppendToFile(string path, string content);
            void UpdateFile(string path, string newContent);
            void DeleteFile(string path);
            void CreateDirectory(string path);
            void DeleteDirectory(string path);
            bool DirectoryExists(string path);
            List<string> ListDirectory(string path);
        }

        public class InMemoryFileSystem : IFileSystem
        {
            private Dictionary<string, string> files = new Dictionary<string, string>();
            private HashSet<string> directories = new HashSet<string> { "/" };

            public void CreateFile(string path, string content)
            {
                if (files.ContainsKey(path))
                {
                    throw new Exception("File already exists.");
                }
                files[path] = content;
            }

            public string ReadFile(string path)
            {
                if (!files.ContainsKey(path))
                {
                    throw new Exception("File not found.");
                }
                return files[path];
            }

            public void AppendToFile(string path, string content)
            {
                if (!files.ContainsKey(path))
                {
                    throw new Exception("File not found.");
                }
                files[path] += content;
            }

            public void UpdateFile(string path, string newContent)
            {
                if (!files.ContainsKey(path))
                {
                    throw new Exception("File not found.");
                }
                files[path] = newContent;
            }

            public void DeleteFile(string path)
            {
                if (!files.ContainsKey(path))
                {
                    throw new Exception("File not found.");
                }
                files.Remove(path);
            }

            public void CreateDirectory(string path)
            {
                if (directories.Contains(path))
                {
                    throw new Exception("Directory already exists.");
                }
                directories.Add(path);
            }

            public void DeleteDirectory(string path)
            {
                if (!directories.Contains(path))
                {
                    throw new Exception("Directory not found.");
                }
                directories.RemoveWhere(d => d.StartsWith(path));
                files = files.Where(f => !f.Key.StartsWith(path)).ToDictionary(f => f.Key, f => f.Value);
            }

            public bool DirectoryExists(string path)
            {
                return directories.Contains(path);
            }

            public List<string> ListDirectory(string path)
            {
                List<string> items = new List<string>();
                foreach (var dir in directories.Where(d => d.StartsWith(path) && d != path))
                {
                    items.Add(dir);
                }
                foreach (var file in files.Keys.Where(f => f.StartsWith(path)))
                {
                    items.Add(file);
                }
                return items;
            }
        }

        public class Process
        {
            public string Name { get; set; }
            public int Priority { get; set; }

            public Process(string name, int priority)
            {
                Name = name;
                Priority = priority;
            }

            public void Run()
            {
                Console.WriteLine($"Process {Name} is running.");
            }
        }

    }
}
