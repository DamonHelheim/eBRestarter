using eBRestarter.Classes.EBInterfaces;
using Serilog;

namespace eBRestarter.Classes.EBFiles
{
    public class Count : ICount
    {

        public T CountFilesInDirectoryI<T>(string directoryPath)
        {
            try
            {
                T count = default!;

                if (typeof(T) == typeof(int))
                {
                    int intCount = 0;
                    string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                    intCount = files.Length;
                    count = (T)(object)intCount;
                }
                else if (typeof(T) == typeof(string))
                {
                    string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                    string fileCountString = files.Length.ToString();
                    count = (T)(object)fileCountString;
                }
                //else
                //{
                //    throw new NotSupportedException("Unsupported count type.");
                //}

                return count;

            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im Zählbereich aufgetreten. Logging...");
                T count = default!;
                return count;
            }


        }


        public T CountNumberOfFilesInSpecifiedDirectories<T>(List<string> directoryPaths)
        {
            try
            {
                T count = default!;

                if (typeof(T) == typeof(int))
                {
                    int intCount = 0;

                    foreach (string directoryPath in directoryPaths)
                    {
                        try
                        {
                            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                            intCount += files.Length;

                        }
                        catch (Exception)
                        {

                        }
                    }

                    count = (T)(object)intCount;
                }

                else if (typeof(T) == typeof(string))
                {
                    int totalCount = 0;

                    foreach (string directoryPath in directoryPaths)
                    {
                        string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                        totalCount += files.Length;
                    }
                    string fileCountString = totalCount.ToString();
                    count = (T)(object)fileCountString;
                }
                //else
                //{
                //    throw new NotSupportedException("Unsupported count type.");
                //}

                return count;

            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im Zählbereich aufgetreten. Logging...");
                T count = default!;
                return count;
            }

        }

    }
}
