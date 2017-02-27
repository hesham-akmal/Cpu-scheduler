using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace CPU_SCHEDULER_FORM
{
    class Process
    {
        public int id;

        public int burstTimeOriginal;
        public int burstTimeLeft;

        public int arriveTime;
        public int priority;

        public int startTime;
        public int endTime;
        public int waitingTime;
        public int TATime;

        public int ranQuantums = 0;

        public Process(int burstTime)
        {
            this.burstTimeLeft = burstTime;
            this.burstTimeOriginal = burstTime;
        }

        //calculates waiting time, then adds it to avg time list
        public void calcWaitingTimeAndAddAvg()
        {
            waitingTime = (endTime - startTime) - burstTimeOriginal;
            //Console.WriteLine("endTime: " + endTime + "startTime: " + startTime + "burstT: " + burstTimeOriginal);
            SCH.allWaitingTimes.Add(waitingTime);
        }
    }

    class DrawTools
    {
        public static SolidBrush RedBrush = new SolidBrush(Color.Red);
        public static SolidBrush WhiteBrush = new SolidBrush(Color.White);
        public static SolidBrush BlackBrush = new SolidBrush(Color.Black);

        public static Font fonty = new Font("Arial", 8);
        public static Pen myPen = new Pen(RedBrush);
    }


    class SCH
    {
        public static List<Process> allProcesses = new List<Process>();

        public static List<Process> arrivedProcesses = new List<Process>();

        public static List<int> allWaitingTimes = new List<int>();

        public static List<Form1> allForms = new List<Form1>();

        public static int i = 1;

        public static int currentTime = 0;

        public static float avgWaitingTime = 0;

        public static int speed = 1000;

        public static int State;

        public static int quantum;

        public static int turn = 0;

        public static void resetALL()
        {
            turn = 0;
            i = 1;
            speed = 1000;
            avgWaitingTime = 0;
            currentTime = 0;
            allWaitingTimes.Clear();
            allProcesses.Clear();
            arrivedProcesses.Clear();
        }

        public static void calcAvgTime()
        {
            float totalWaitingTime = 0;
            foreach (int a in allWaitingTimes)
                totalWaitingTime += a;

            avgWaitingTime = totalWaitingTime / (i - 1);

            //Console.WriteLine("totalWaitingTime: " + totalWaitingTime + "processes: " + processes);
        }
    }


    class Scheduler
    {

        Queue<Process> arrivedProcessesQ = new Queue<Process>();


        public void createProcesses()
        {
            while (true)
            {
                Console.Write("\nEnter Process " + SCH.i + " burst time: ");
                Process p = new Process(Convert.ToInt32(Console.ReadLine()));
                Console.Write("Enter Process " + SCH.i + " arrival time: ");
                p.arriveTime = Convert.ToInt32(Console.ReadLine());

                if (SCH.State == 4 || SCH.State == 5)
                {
                    Console.Write("Enter Process " + SCH.i + " priority: ");
                    p.priority = Convert.ToInt32(Console.ReadLine());
                }

                p.id = SCH.i++;
                SCH.allProcesses.Add(p);

                Console.WriteLine("Press 0 to finish, anything else to add another process");
                char chary = Console.ReadKey(true).KeyChar;
                if (chary == '0')
                    break;
            }

            Form1 newForm = new Form1();

            if (SCH.State == 4 || SCH.State == 5)//hide priority shit if priority mode
                newForm.showPriority();

            SCH.allForms.Add(newForm);

            new Thread(delegate ()
            {
                Application.Run(newForm);
            }).Start();

            Console.Write("\nGantt Chart: \n ");

            int countPS = 0;
            int oldPid = -1;

            //sort at first
            if (SCH.State == 3)
                SCH.allProcesses.Sort(delegate (Process x, Process y)
                {
                    return x.burstTimeOriginal.CompareTo(y.burstTimeOriginal);
                });
            else if (SCH.State == 1)
                SCH.allProcesses.Sort(delegate (Process x, Process y)
                   {
                       return x.arriveTime.CompareTo(y.arriveTime);
                   });
            else if (SCH.State == 5)
                SCH.allProcesses.Sort(delegate (Process x, Process y)
                {
                    return x.priority.CompareTo(y.priority);
                });

            while (SCH.allProcesses.Count > 0) // run until all processes finish
            {
                Thread.Sleep(SCH.speed);

                foreach (Process a in SCH.allProcesses)
                {
                    if (a.burstTimeLeft <= 0)
                        continue;

                    if (SCH.State != 6)
                    {
                        if (SCH.arrivedProcesses.Contains(a))
                            continue;

                        if (a.arriveTime <= SCH.currentTime)
                        {
                            SCH.arrivedProcesses.Add(a);
                            a.startTime = SCH.currentTime;
                        }
                    }
                    else
                    {
                        if (arrivedProcessesQ.Contains(a))
                            continue;

                        if (a.arriveTime <= SCH.currentTime)
                        {
                            a.startTime = SCH.currentTime;
                            arrivedProcessesQ.Enqueue(a);
                        }
                    }
                }

                countPS++;

                int Xstart = (20) * SCH.currentTime;
                newForm.g.FillRectangle(DrawTools.BlackBrush, new Rectangle(Xstart - 1, 90, 1, 10));
                newForm.g.DrawString(SCH.currentTime.ToString(), DrawTools.fonty, DrawTools.BlackBrush, Xstart - 10, 100);

                //if non arrived yet //if gap
                if ((SCH.arrivedProcesses.Count == 0 && SCH.State != 6) || arrivedProcessesQ.Count == 0 && SCH.State == 6)
                {
                    //print gap
                    Console.Write(" . ");
                    newForm.g.FillRectangle(DrawTools.WhiteBrush, new Rectangle((20) * SCH.currentTime, 50, 20, 50));
                    newForm.g.FillRectangle(DrawTools.BlackBrush, new Rectangle(Xstart - 1, 50, 2, 50));
                    SCH.currentTime++;
                    continue;
                }

                //sort by shortest burst every sec if preemptive
                if (SCH.State == 2)
                    SCH.arrivedProcesses.Sort(delegate (Process x, Process y)
                {
                    return x.burstTimeLeft.CompareTo(y.burstTimeLeft);
                });
                else if (SCH.State == 4)
                    SCH.arrivedProcesses.Sort(delegate (Process x, Process y)
                    {
                        return x.priority.CompareTo(y.priority);
                    });

                //Run first P in arrived List for one time unit
                Process P;

                bool quantumaDone = false;

                if (SCH.State != 6)
                    P = SCH.arrivedProcesses[0];
                else // priority
                {
                    if (arrivedProcessesQ.Peek().ranQuantums == SCH.quantum)
                    {
                        quantumaDone = true;
                        newForm.g.FillRectangle(DrawTools.BlackBrush, new Rectangle(Xstart - 1, 50, 2, 50));
                        arrivedProcessesQ.Peek().ranQuantums = 0;
                        arrivedProcessesQ.Enqueue(arrivedProcessesQ.Dequeue());
                    }
                    P = arrivedProcessesQ.Peek();
                    arrivedProcessesQ.Peek().ranQuantums++;
                }

                Console.Write("P" + P.id + " ");//run P this second

                newForm.g.FillRectangle(DrawTools.RedBrush, new Rectangle(Xstart, 50, 20, 50));

                if (P.id != oldPid || (SCH.State == 6 && quantumaDone))
                {
                    newForm.g.DrawString(("P" + P.id), DrawTools.fonty, DrawTools.BlackBrush, Xstart, 50);
                    newForm.g.FillRectangle(DrawTools.BlackBrush, new Rectangle(Xstart - 1, 50, 2, 50));
                    oldPid = P.id;
                }

                P.burstTimeLeft--;

                //if process done then mark as done
                if (P.burstTimeLeft == 0)
                {
                    P.endTime = SCH.currentTime + 1;
                    P.calcWaitingTimeAndAddAvg();
                    SCH.allProcesses.Remove(P);

                    if (SCH.State != 6)
                        SCH.arrivedProcesses.Remove(P);
                    else
                        arrivedProcessesQ.Dequeue();

                    if (SCH.State == 3)
                        SCH.arrivedProcesses.Sort(delegate (Process x, Process y)
                    {
                        return x.burstTimeOriginal.CompareTo(y.burstTimeOriginal);
                    });
                    else if (SCH.State == 5)
                        SCH.arrivedProcesses.Sort(delegate (Process x, Process y)
                        {
                            return x.priority.CompareTo(y.priority);
                        });
                    else if (SCH.State == 1)
                        SCH.arrivedProcesses.Sort(delegate (Process x, Process y)
                        {
                            return x.arriveTime.CompareTo(y.arriveTime);
                        });
                }
                SCH.currentTime++;
            }
            //All processes finish here
            SCH.calcAvgTime();

            Console.Write("\n");
            for (int H = 0; H < countPS + 1; H++)
                Console.Write(H.ToString("00") + " ");
            Console.Write("\n");

            Console.WriteLine("\nAverage Waiting time: " + SCH.avgWaitingTime + "\n\n");
            newForm.label4.Text = "Average Waiting time: " + SCH.avgWaitingTime;
        }
    }

    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true)
            {

                try
                {

                    Console.WriteLine("Press 1 for FCFS, 2 for SJF, 3 for Priority, 4 for Round Robin");
                    switch (Console.ReadKey(true).KeyChar)
                    {
                        case '1':
                            Console.WriteLine("FCFS");
                            SCH.State = 1;
                            new Scheduler().createProcesses();
                            break;

                        case '2':
                            while (true)
                            {
                                Console.WriteLine("SJF: Press 1 for non preemptive, 2 for preemptive");

                                char c = Console.ReadKey(true).KeyChar;

                                if (c == '2')
                                {
                                    Console.WriteLine("Preemptive");
                                    SCH.State = 2;
                                    break;
                                }
                                else if (c == '1')
                                {
                                    Console.WriteLine("Non Preemptive");
                                    SCH.State = 3;
                                    break;
                                }
                            }

                            new Scheduler().createProcesses();
                            break;

                        case '3':
                            while (true)
                            {
                                Console.WriteLine("Priority: Press 1 for non preemptive, 2 for preemptive");

                                char c = Console.ReadKey(true).KeyChar;

                                if (c == '2')
                                {
                                    Console.WriteLine("Preemptive");
                                    SCH.State = 4;
                                    break;
                                }
                                else if (c == '1')
                                {
                                    Console.WriteLine("Non Preemptive");
                                    SCH.State = 5;
                                    break;
                                }
                            }

                            new Scheduler().createProcesses();
                            break;

                        case '4':
                            SCH.State = 6;

                            Console.Write("Round Robin\nEnter Quantum time: ");
                            SCH.quantum = Convert.ToInt32(Console.ReadLine());

                            new Scheduler().createProcesses();
                            break;
                    }

                    Console.WriteLine("Press anything to restart.");

                    Console.ReadKey(true);
                    foreach (Form1 a in SCH.allForms)
                        if (a.Visible)
                            a.Visible = false;

                    SCH.resetALL();

                    Console.WriteLine("\n");
                }
                catch
                {
                    MessageBox.Show("EXCEPTION! Program restarting..", "ERROR",
     MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine("\n");
                    continue;
                }

            }
        }




    }

    partial class Form1
    {
        public void hidePriority()
        {
            Prioritybox.Visible = false;
            label3.Visible = false;
        }
        public void showPriority()
        {
            Prioritybox.Visible = true;
            label3.Visible = true;
        }
    }
}