using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Area51Elevator
{

    enum BaseFloors { G, S, T1, T2 };
    class Elevator
    {
        
        static ManualResetEvent elevatorEvent = new ManualResetEvent(false);
        
        const int maxPeopleInElevator = 1;

        static Semaphore semaphore;

        static Random rand = new Random();

        static List<Agent> agentInElevator;
        
        public Elevator()
        {
            semaphore = new Semaphore(1, maxPeopleInElevator);
            agentInElevator = new List<Agent>();
        }

        static int currentFloorTime;
        static int lastFloorTime = 0;

        public static BaseFloors PushRandomFloorButton()
        {

            int floor = rand.Next(1, 5);
            if (floor == 1)
            {

                return BaseFloors.S;
            }
            else if (floor == 2)
            {

                return BaseFloors.T1;
            }
            else if (floor == 3)
            {

                return BaseFloors.T2;
            }
            else
            {
                return BaseFloors.G;
            }

        }
        static bool firstEntry = true;
        static BaseFloors currentFloor;
        Thread elevatorThread;
        bool threadStart = true;
        static int counter = 0;
        public void EnterElevator(Agent agent)
        {

            //starts the thread with the first agent
            if (threadStart == true)
            {
                threadStart = false;
                elevatorThread = new Thread(InsideElevator);
                
                elevatorThread.Start();
            }


            semaphore.WaitOne();

            lock (agentInElevator)
            {
                agentInElevator.Add(agent);
                Console.WriteLine(agentInElevator[0].AgentNumber + " entered the elevator.");
                //using an if because the thread is started  when it is used for the first time
                if(counter > 0)
                {
                    elevatorEvent.Set();
                    InsideElevator();
                }

            }


        }


        private static void InsideElevator()
        {

            //Setting the initial floor at the start of the workday
            if (firstEntry == true)
            {
                currentFloor = BaseFloors.G;
            }
            bool inTheElevator = true;


            //1s per floor time
            switch (agentInElevator[0].CurrentAgentFloor)
            {
                case Agent.AgentCurrentFloor.T2:
                    currentFloorTime = 3000;
                    Thread.Sleep(Math.Abs(currentFloorTime - lastFloorTime));
                    lastFloorTime = currentFloorTime;
                    break;
                case Agent.AgentCurrentFloor.T1:
                    currentFloorTime = 2000;
                    Thread.Sleep(Math.Abs(currentFloorTime - lastFloorTime));
                    lastFloorTime = currentFloorTime;
                    break;
                case Agent.AgentCurrentFloor.S:
                    currentFloorTime = 1000;
                    Thread.Sleep(Math.Abs(currentFloorTime - lastFloorTime));
                    lastFloorTime = currentFloorTime;
                    break;
                case Agent.AgentCurrentFloor.G:
                    currentFloorTime = 0;
                    Thread.Sleep(Math.Abs(currentFloorTime - lastFloorTime));
                    lastFloorTime = currentFloorTime;
                    break;
            }
            var clickedElevatorButton = PushRandomFloorButton();

            //While the agent is inside the elevator this executes
            while (inTheElevator == true)
            {
                int randomAction = rand.Next(1, 4);

                switch (clickedElevatorButton)
                {
                    case BaseFloors.T2:
                        currentFloor = BaseFloors.T2;
                        currentFloorTime = 3000;
                        Thread.Sleep(Math.Abs(currentFloorTime - lastFloorTime));
                        lastFloorTime = currentFloorTime;
                        Console.WriteLine(agentInElevator[0].AgentNumber + " arrived at level T2");
                        if (agentInElevator[0].AgentLevel == "Top-Secret")
                        {
                            inTheElevator = false;
                            agentInElevator[0].CurrentAgentFloor = Agent.AgentCurrentFloor.T2;
                        }
                        else
                        {
                            clickedElevatorButton = GenerateNewFloor(randomAction - 1);
                        }
                        break;
                    case BaseFloors.T1:
                        currentFloor = BaseFloors.T1;
                        currentFloorTime = 2000;
                        Thread.Sleep(Math.Abs(currentFloorTime - lastFloorTime));
                        lastFloorTime = currentFloorTime;
                        Console.WriteLine(agentInElevator[0].AgentNumber + " arrived at level T1");
                        if (agentInElevator[0].AgentLevel == "Top-Secret")
                        {
                            inTheElevator = false;
                            agentInElevator[0].CurrentAgentFloor = Agent.AgentCurrentFloor.T1;
                        }
                        else
                        {
                            clickedElevatorButton = GenerateNewFloor(randomAction - 2);
                        }
                        break;
                    case BaseFloors.S:
                        currentFloor = BaseFloors.S;
                        currentFloorTime = 1000;
                        Thread.Sleep(Math.Abs(currentFloorTime - lastFloorTime));
                        lastFloorTime = currentFloorTime;
                        Console.WriteLine(agentInElevator[0].AgentNumber + " arrived at level S");
                        if (agentInElevator[0].AgentLevel == "Secret" || agentInElevator[0].AgentLevel == "Top-Secret")
                        {
                            inTheElevator = false;
                            agentInElevator[0].CurrentAgentFloor = Agent.AgentCurrentFloor.S;
                        }
                        else
                        {
                            clickedElevatorButton = BaseFloors.G;
                        }
                        break;

                    case BaseFloors.G:
                    default:
                        currentFloor = BaseFloors.G;
                        currentFloorTime = 0;
                        Thread.Sleep(Math.Abs(currentFloorTime - lastFloorTime));
                        lastFloorTime = currentFloorTime;
                        Console.WriteLine(agentInElevator[0].AgentNumber + " arrived at ground level G.");
                        agentInElevator[0].CurrentAgentFloor = Agent.AgentCurrentFloor.G;
                        inTheElevator = false;
                        break;
                }


            }
            //Stops the elevator thread
            elevatorEvent.Reset();
            counter++;
            LeaveElevator(agentInElevator[0]);
            
        }

        public static void LeaveElevator(Agent agent)
        {
            Console.WriteLine("The door opens and the agent gets of the elevator.");

            lock (agentInElevator)
            {
                agentInElevator[0].enteredElevator = false;
                agentInElevator.Remove(agent);
                
            }

            semaphore.Release();
            
        }


        //Generates a new random floor in case the agent doesn't have the needed credentials
        public static BaseFloors GenerateNewFloor(int randNum)
        {
            BaseFloors elevatorAction;
            switch (randNum)
            {
                case 1:
                    elevatorAction = BaseFloors.G;
                    break;
                case 2:
                    elevatorAction = BaseFloors.S;
                    break;
                case 3:
                    elevatorAction = BaseFloors.T1;
                    break;
                case 4:
                default:
                    elevatorAction = BaseFloors.T2;
                    break;
            }

            return elevatorAction;
        }

    }
}

