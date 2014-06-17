# Rumle.Golang

A small POC project where i try implementing Golang features in C#. 

This is just something that I have put togehter in an evening after reading a bit about Go, not something that I would use in a real production environment. Each "goroutine" runs in its own background thread.



### Channel&lt;T&gt;

The following peace of C# code is based on Sameer Ajmanis ping-pong example from his __Advanced Go Concurrency Patterns__ talk: 

    class Program
	{
        class Ball
        {
            public int Hits { get; set; }
        }

        static void Main(string[] args)
        {
            var table = new Channel<Ball>();
            Go.Run(() => Player("ping", table));
            Go.Run(() => Player("pong", table));
            table.Send(new Ball());
            Thread.Sleep(1000);
            table.Receive();
        }
        
        static void Player(string name, Channel<Ball> table)
        {
            while (true)
            {
                var ball = table.Receive();
                ball.Hits++;
                Console.WriteLine(name + " " + ball.Hits);
                Thread.Sleep(100);
                table.Send(ball);
            }
        }
	}
	
The original Go program can be found here: http://talks.golang.org/2013/advconc.slide#6


### Select

The follow C# code is based on an example by @mmcgrana

        static void Main(string[] args)
        {
            var c1 = new Channel<string>();
            var c2 = new Channel<string>();

            Go.Run(() =>
                {
                    Thread.Sleep(1000);
                    c1.Send("one");
                });

            Go.Run(() =>
                {
                    Thread.Sleep(2000);
                    c1.Send("two");
                });

            for (int i = 0; i < 2; i++)
            {
                Go.Select()
                  .Case(c1, _ => Console.WriteLine("received " + _))
                  .Case(c2, _ => Console.WriteLine("received " + _))
                  .Run();
            }
        }
        
The original Go program can be found here: https://gobyexample.com/select

