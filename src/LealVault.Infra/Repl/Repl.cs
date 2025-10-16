using System.Text;
using System.Threading.Tasks;
using TextCopy;

namespace LealVault.Infra.Repl
{
    /// <summary>
    /// A small console REPL that supports:
    /// - input editing (insert, backspace)
    /// - left/right cursor movement
    /// - up/down history navigation
    /// - basic prompt handling
    /// </summary>
    public sealed class Repl
    {
        private const string Prompt = "> ";

        private readonly List<string> _history;
        private readonly Dictionary<ConsoleKey, Func<ConsoleKeyInfo, ReplResult>> _inputKeyActions;
        private readonly CancellationToken _token;

        private StringBuilder _input = new();
        private int _cursor; // insertion cursor within _input (0.._input.Length)
        private int _historyIndex; // index into _history (0..Count), Count means "new empty"

        /// <summary>
        /// Instantiates a new REPL.
        /// </summary>
        public Repl(CancellationToken token)
        {
            _history = [];
            _token = token;
            _inputKeyActions = new()
            {
                { ConsoleKey.Backspace, HitBackspace },
                { ConsoleKey.Enter, HitEnter },
                { ConsoleKey.UpArrow, HitUp },
                { ConsoleKey.DownArrow, HitDown },
                { ConsoleKey.LeftArrow, HitLeft },
                { ConsoleKey.RightArrow, HitRight },
                { ConsoleKey.V, HitV }
            };
        }

        /// <summary>
        /// Runs the REPL loop once and returns the entered line (does not loop forever).
        /// </summary>
        public async Task<string> Run(ExecutionResult? lastExecutionResult)
        {
            _historyIndex = _history.Count;
            _input = new StringBuilder();
            _cursor = 0;

            if (lastExecutionResult is not null && lastExecutionResult.VaultIsOpen)
            {
                var fileName = Path.GetFileName(lastExecutionResult.VaultPath);
                var dirtyMark = lastExecutionResult.VaultIsDirty ? "*" : "";
                $"{fileName}{dirtyMark} ".Write(ConsoleColor.Gray);
            }

            Prompt.Write(ConsoleColor.Cyan);

            while (!_token.IsCancellationRequested)
            {
                ConsoleKeyInfo keyInfo;

                if (!Console.KeyAvailable)
                    continue;

                try
                {
                    keyInfo = Console.ReadKey(true);
                }
                catch (Exception)
                {
                    continue;
                }

                if (!_inputKeyActions.TryGetValue(keyInfo.Key, out var action))
                {
                    // normal printable character (control chars ignored)
                    if (!char.IsControl(keyInfo.KeyChar))
                        InsertChar(keyInfo.KeyChar);

                    continue;
                }

                var replResult = action(keyInfo);

                if (replResult.Override)
                {
                    // replace whole input with provided value
                    RedrawInput(replResult.Value ?? string.Empty);
                }

                if (replResult.Return)
                {
                    var line = _input.ToString();
                    if (!string.IsNullOrEmpty(line))
                        _history.Add(line);
                    Console.WriteLine();
                    return line;
                }
            }

            return string.Empty;
        }

        #region [ Input Key Actions ]

        private ReplResult HitBackspace(ConsoleKeyInfo keyInfo)
        {
            if (_cursor > 0 && _input.Length > 0)
                RemoveCharBeforeCursor();

            return ReplResult.Empty;
        }

        private ReplResult HitEnter(ConsoleKeyInfo info) => ReplResult.Enter;

        private ReplResult HitUp(ConsoleKeyInfo info)
        {
            if (_history.Count == 0)
                return new ReplResult(string.Empty, overrideLine: true);

            // move to older entry
            if (_historyIndex > 0)
                _historyIndex--;
            else
                _historyIndex = 0;

            return new ReplResult(_history[_historyIndex], overrideLine: true);
        }

        private ReplResult HitDown(ConsoleKeyInfo info)
        {
            if (_history.Count == 0)
                return new ReplResult(string.Empty, overrideLine: true);

            // move to newer entry
            if (_historyIndex < _history.Count - 1)
            {
                _historyIndex++;
                return new ReplResult(_history[_historyIndex], overrideLine: true);
            }

            // past the last entry -> blank line
            _historyIndex = _history.Count;
            return new ReplResult(string.Empty, overrideLine: true);
        }

        private ReplResult HitLeft(ConsoleKeyInfo info)
        {
            if (_cursor > 0)
            {
                // move left one position
                try
                {
                    if (Console.CursorLeft > 0)
                        Console.CursorLeft -= 1;
                    else if (Console.CursorTop > 0)
                    {
                        // move up to previous line end
                        Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
                    }
                }
                catch { }

                _cursor--;
            }

            return ReplResult.Empty;
        }

        private ReplResult HitRight(ConsoleKeyInfo info)
        {
            if (_cursor < _input.Length)
            {
                try
                {
                    if (Console.CursorLeft < Console.BufferWidth - 1)
                        Console.CursorLeft += 1;
                    else
                    {
                        Console.SetCursorPosition(0, Console.CursorTop + 1);
                    }
                }
                catch { }

                _cursor++;
            }

            return ReplResult.Empty;
        }

        private ReplResult HitV(ConsoleKeyInfo info)
        {
            var hitControl = info.Modifiers.HasFlag(ConsoleModifiers.Control);

            if (!hitControl)
            {
                InsertChar(info.KeyChar);
                return ReplResult.Empty;
            }

            var clipboardContent = ClipboardService.GetText();

            if (string.IsNullOrEmpty(clipboardContent))
                return ReplResult.Empty;

            return new ReplResult(clipboardContent);
        }
        #endregion

        #region [ Utilities ]
        private void InsertChar(char c)
        {
            // Insert into the StringBuilder at the cursor position
            _input.Insert(_cursor, c);

            // Write the suffix (from cursor) to update display, then reposition cursor after inserted char
            var suffix = _input.ToString().Substring(_cursor);
            Console.Write(suffix);

            // move cursor back to after the inserted character
            var moveBack = suffix.Length - 1;
            if (moveBack > 0) Console.Write(new string('\b', moveBack));

            _cursor++;
        }

        private void RemoveCharBeforeCursor()
        {
            int removeIndex = _cursor - 1;
            _input.Remove(removeIndex, 1);

            // move console cursor back one position
            Console.Write("\b");

            // print the rest of the buffer from removeIndex
            var suffix = _input.ToString().Substring(removeIndex);
            Console.Write(suffix);

            // print a space to erase leftover char (if any), then move cursor back to correct place
            Console.Write(' ');
            Console.Write(new string('\b', suffix.Length + 1));

            _cursor--;
        }

        private void RedrawInput(string newInput)
        {
            // Best-effort: assume prompt is on current line.
            try
            {
                int promptLeft = Prompt.Length;
                int curTop = Console.CursorTop;
                Console.SetCursorPosition(promptLeft, curTop);

                // clear existing content
                Console.Write(new string(' ', _input.Length));
                Console.SetCursorPosition(promptLeft, curTop);

                // write new input
                Console.Write(newInput);
            }
            catch
            {
                // fall back to simple write if cursor positioning fails
                Console.WriteLine();
                Console.Write(Prompt + newInput);
            }

            _input = new StringBuilder(newInput);
            _cursor = _input.Length;
        }

        #endregion
    }
}