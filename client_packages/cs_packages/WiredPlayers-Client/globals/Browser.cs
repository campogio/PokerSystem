using RAGE;
using RAGE.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using WiredPlayers_Client.chat;

namespace WiredPlayers_Client.globals
{
    public class Browser : Events.Script
    {
        public static string CurrentLanguage;
        public static HtmlWindow CustomBrowser;

        private static object[] Parameters;
        private static string ClosingEvent;

        public Browser()
        {
            try
            {
                Events.Add("destroyBrowser", DestroyBrowserEvent);

                Events.OnBrowserDomReady += OnBrowserDomReady;
            }
            catch (Exception e)
            {
                RAGE.Ui.Console.Log(ConsoleVerbosity.Info, e.StackTrace, true);
            }
        }

        public static void CreateBrowser(string html, string closingFunction, params object[] args)
        {
            try
            {
                if (CustomBrowser == null)
                {
                    // Get the closing function
                    ClosingEvent = closingFunction;

                    // Save the rest of the parameters
                    Parameters = args;

                    // Create the browser
                    CustomBrowser = new HtmlWindow("package2://statics/html/" + html);
                }
                else DestroyBrowserEvent(null);
            }
            catch (Exception e)
            {
                RAGE.Ui.Console.Log(ConsoleVerbosity.Info, e.StackTrace, true);
            }
        }

        public static void ExecuteFunction(string function, params object[] args)
        {
            try
            {
                if (CustomBrowser == null)
                    return;
                // Call the function with the parameters
                CustomBrowser.Call(function, args);
            }
            catch (Exception e)
            {
                RAGE.Ui.Console.Log(ConsoleVerbosity.Info, e.StackTrace, true);
            }
        }

        public static void ExecuteJsFunction(string function)
        {
            try
            {
                if (CustomBrowser == null)
                    return;
                CustomBrowser.ExecuteJs(function);
            }
            catch (Exception e)
            {
                RAGE.Ui.Console.Log(ConsoleVerbosity.Info, e.StackTrace, true);
            }
        }

        public static void ForceBrowserClose()
        {
            try
            {
                if (!string.IsNullOrEmpty(ClosingEvent))
                {
                    // Call the close function
                    Events.CallLocal(ClosingEvent);
                    ClosingEvent = string.Empty;
                }
            }
            catch (Exception e)
            {
                RAGE.Ui.Console.Log(ConsoleVerbosity.Info, e.StackTrace, true);
            }
        }

        public static void DestroyBrowserEvent(object[] args)
        {
            try
            {
                if (!ChatManager.Opened)
                {
                    // Disable the cursor and enable the chat
                    Cursor.Visible = false;
                }

                // Destroy the browser
                CustomBrowser?.Destroy();
                CustomBrowser = null;
            }
            catch (Exception e)
            {
                RAGE.Ui.Console.Log(ConsoleVerbosity.Info, e.StackTrace, true);
            }
        }

        private static void OnBrowserDomReady(HtmlWindow window)
        {
            try
            {
                if (CustomBrowser != null && window.Id == CustomBrowser.Id && Parameters != null)
                {
                    // Enable the cursor and disable the chat
                    Cursor.Visible = true;

                    // Add the language to the parameters
                    List<object> parametersList = Parameters.ToList();
                    parametersList.Insert(0, CurrentLanguage);

                    // Remove the parameters
                    Parameters = null;

                    // Call the function passed as parameter
                    ExecuteFunction("initializeMessages", parametersList.ToArray());
                }
            }
            catch (Exception e)
            {
                RAGE.Ui.Console.Log(ConsoleVerbosity.Info, e.StackTrace, true);
                throw;
            }
        }
    }
}