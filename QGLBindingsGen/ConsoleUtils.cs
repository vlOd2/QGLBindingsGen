using System.Text;

namespace QGLBindingsGen;

internal static class ConsoleUtils
{
    // TODO: Add support for multiple animations
    // TODO: Refactor this crap to a separate class
    private static async Task ShowAnimationForTask(string name, Task task)
    {
        TextWriter stdOut = Console.Out;
        StringWriter tempOut = new();
        Console.SetOut(tempOut);
        stdOut.Write(name);

        Console.CursorVisible = false;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        stdOut.Write(" [");
        (int left, int top) progressPos = Console.GetCursorPosition();
        stdOut.Write("".PadLeft(5));
        stdOut.WriteLine("]");
        (int left, int top) endPos = Console.GetCursorPosition();
        
        async Task loadingAnim(int i)
        {
            Console.SetCursorPosition(progressPos.left + 1, progressPos.top);
            stdOut.Write((new string('.', i) + '*').PadRight(3, '.'));
            await Task.Delay(150);
        }

        bool reverse = false;
        while (!task.IsCompleted)
        {
            Console.ForegroundColor = ConsoleColor.White;

            if (reverse)
            {
                for (int i = 2; i >= 0 && !task.IsCompleted; i--)
                    await loadingAnim(i);
            }
            else
            {
                for (int i = 0; i < 3 && !task.IsCompleted; i++)
                    await loadingAnim(i);
            }

            StringBuilder outBuilder = tempOut.GetStringBuilder();
            if (outBuilder.Length > 0)
            {
                StringReader reader = new(outBuilder.ToString());
                outBuilder.Clear();
                Console.ResetColor();
                Console.SetCursorPosition(endPos.left, endPos.top);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    stdOut.WriteLine(line);
                    endPos = Console.GetCursorPosition();
                }
            }

            reverse = !reverse;
        }

        Console.SetCursorPosition(progressPos.left, progressPos.top);
        if (task.IsCompletedSuccessfully)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            stdOut.Write("  \u2713  ");
        }
        else if (task.IsFaulted)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            stdOut.Write("  X  ");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            stdOut.Write("  ?  ");
        }

        Console.CursorVisible = true;
        Console.SetCursorPosition(endPos.left, endPos.top);
        Console.ResetColor();
        Console.SetOut(stdOut);
    }

    public static void WriteError(object obj)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(obj);
        Console.ResetColor();
    }

    public static async Task RunTask(string name, Task task)
    {
        await ShowAnimationForTask(name, task);
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            WriteError(ex);
            throw;
        }
    }

    public static async Task<T> RunTask<T>(string name, Task<T> task)
    {
        await ShowAnimationForTask(name, task);
        try
        {
            return await task;
        }
        catch (Exception ex)
        {
            WriteError(ex);
            throw;
        }
    }
}
