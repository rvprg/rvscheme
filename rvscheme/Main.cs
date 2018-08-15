using System;
using System.IO;
using System.Text;

namespace rvscheme
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            System.Console.WriteLine ("Welcome to RV-Scheme (2011-04-24), a primitive LISP system.\r\n");
            // There should be some standard lib... 
            System.Console.Write ("Loading standard lib: ");

            var evaluator = new Evaluator ();
            var lexer = new Lexer ();

            // This is just for fun, a snippet from SICP book :-)
            string s1 = "(define (sqrt-iter guess x) (if (good-enough? guess x) guess (sqrt-iter (improve guess x) x)))" + "(define (improve guess x) (average guess (/ x guess)))" + "(define (average x y)(/ (+ x y) 2))" + "(define (good-enough? guess x) (< (abs (- (square guess) x)) 0.001))" + "(define (sqrt x) (sqrt-iter 1.0 x))" + "(define (square x) (* x x)) (define the-continuation #f)(define (test)(let ((i 0)) (call/cc (lambda (k) (set! the-continuation k)))(set! i (+ i 1)) i)) (define (factorial n i) (if (< 0 n) (factorial (- n 1) (* i n)) i)) (define (f n) (if (< 0 n) (* n (f (- n 1))) 1))";
            var ts = lexer.GetTokens (s1, "sqrt.csm");
            var ps = Parser.Parse (ts);
            evaluator.Execute (ps, res => null);

            ConsoleColor defColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine ("Done.\r\n");
            System.Console.ForegroundColor = defColor;

            while (true)
            {
                try
                {
                    System.Console.Write ("> ");
                    var expr = System.Console.ReadLine ();
                    var program = Parser.Parse (lexer.GetTokens (expr, "repl"));
                    evaluator.Execute (program, res =>
                    {
                        if (res == null)
                            res = ScmUnassigned.Instance;
                        System.Console.ForegroundColor = ConsoleColor.Cyan;
                        System.Console.WriteLine (String.Format ("\n=> {0}\n", res));
                        System.Console.ForegroundColor = defColor;
                        return null;
                    });

                }
                catch (ParserException e)
                {
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine ("\r\nParser error: \n  " + e.Message);
                    System.Console.ForegroundColor = defColor;
                }
                catch (EvaluatorException e)
                {
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine ("\r\nEvaluation error: \n  " + e.Message);
                    System.Console.ForegroundColor = defColor;
                }
                catch (Exception e)
                {
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine ("\r\nUh oh, internal error: \n  " + e.Message);
                    System.Console.ForegroundColor = defColor;
                }
            }
        }
    }
}

