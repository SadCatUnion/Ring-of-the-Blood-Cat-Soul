namespace FSM.Exceptions
{
    public static class ExceptionFormatter
    {
        public static string Format(string context, string problem, string solution)
        {
            string message = "\n";
            if (context != null)
            {
                message += "Context: " + context + "\n";
            }
            if (problem != null)
            {
                message += "Context: " + problem + "\n";
            }
            if (solution != null)
            {
                message += "Context: " + solution + "\n";
            }
            return message;
        }
    }
}