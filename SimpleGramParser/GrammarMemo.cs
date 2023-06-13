namespace SimpleGramParser
{
    //გრამატიკა სესიაზე არ უნდა იყოს დამოკიდებული
    public class GrammarMemo
    {
        private static GrammarMemo _instance;
        private static readonly object SyncRoot = new object();

        public static GrammarMemo Instance
        {
            get
            {
                lock (SyncRoot) //thread safe singleton
                {
                    if (_instance != null)
                        return _instance;
                    _instance = new GrammarMemo();
                }

                return _instance;
            }
        }

        public Grammar Grammar { get; }
        //public clsPickingBinops PickingBinOps { get { return ppickingBinOps; } }

        //რეგულარული გამოსახულება სტრიქონისთვის
        //(\x22|\x27)((?!\1).|\1{2})*\1

        //მაგალითი
        //"x+y+foo(z)*abc(fun(a+bbb)*y)"
        //გასაანალიზებელი ტექსტის გრამატიკა უნდა იყოს შემდეგი

        //<ტექსტი> ::= <გამოსახულება>  ( [ ';' ] | [ ';' <გამოსახულება> ... ] ) --ეს ნიშნავს, რომ გამოსახულება შეიძლება იყოს ერთი ან რამდენიმე და ერთმანეთისაგან გამოიყოფა ; სიმბოლოთი, თუ მხოლოდ ერთი გამოსახულებაა შეიძლება მთავრდებოდეს ;-ით შეიძლება არა ამას მნიშვნელობა არ უნდა ჰქონდეს
        //<გამოსახულება> ::= <ცვლადი> | <ფუნქცია> | <კონსტანტა> | '(' <გამოსახულება> ')'
        //<ცვლადი> ::= <იდენტიფიკატორი>
        //<ფუქცია> ::= <იდენტიფიკატორი> '(' <გამოსახულება> | <არაფერი> ')'
        //<იდენტიფიკატორი> ::= სიმბოლოებისა, ციფრების და _ თანმიმდევრობა, რომელიც აუცილებლად იწყება ციფრისაგან განსხვავებული სიმბოლოთი
        //              შეესაბამება რეგულარული გამოსახულება @"[A-Za-z_]\w*"
        //<კონსტანტა> ::= <რიცხვი> | <სტრიქონი>
        //<რიცხვი> ::= <მთელი> | <ათწილადი>
        //<მთელი> ::= <ციფრი> [ <ციფრი> ... ]
        //<ათწილადი> ::= (<მთელი> '.' <მთელი>) | ('.' <მთელი>) | (<მთელი> '.')
        //<სტრიქონი> ::= ('"' <ნებისმიერი სიმბოლო სტრიქონის დაბოლოების გარდა და გადუბლირებული "-ით> '"') | (''' <ნებისმიერი სიმბოლო სტრიქონის დაბოლოების გარდა და გადუბლირებული '-ით> ''')
        //              შეესაბამება რეგულარული გამოსახულება @"(\x22|\x27)((?!\1).|\1{2})*\1"


//ითვლება, რომ კომენტარები და #სიმბოლოთი დაწყებული ბრძანებები წინასწარ უნდა იყოს ტექსტიდან ამოყრილი

        private GrammarMemo()
        {
//      Grammar = new Grammar(
//        @"
//Program => Statement Program | Statement
//Statement => Command Semicolon | Block
//Block => Command OpenCurlyBracket BlockBody CloseCurlyBracket
//BlockBody => Statement BlockBody | Statement
//Command => Word Command | Word
//Word => String | Name
//Semicolon => ;
//OpenCurlyBracket => {
//CloseCurlyBracket => }
//String => \""[^\""]*\""
//Name => [^\s{};\""]+
//", "Program");

            //მჭირდებოდა ც#-ის მარტივი ანალიზატორი, სამწუხაროდ მარტივი არ გამოვიდა და გართულებას აზრი არ ჰქონდა. ამიტომ გავჩერდი.
            //მომავალში ასეთი კოდი გამოდგება მხოლოდ მარტივი ანალიზატორებისათვის.
            Grammar = new Grammar(
                @"
Program => Statement ...
Statement => Block | Command Semicolon
Block => Command OpenCurlyBracket CloseCurlyBracket | Command OpenCurlyBracket Statement ... CloseCurlyBracket
Command => Word ...
Word => String | Name | DataBlock
DataBlock => OpenCurlyBracket DataList CloseCurlyBracket
DataList => DataItem Comma DataList | DataItem
DataItem => String | Name
Semicolon => ;
OpenCurlyBracket => {
CloseCurlyBracket => }
String => \""[^\""]*\""
Name => [^\s{};\""]+
", "Program");


            //რადგან რიცხვის განმარტებაში გვაქვს მინუს ნიშნის არსებობის ვარიანტი, ამოტომ უნარული ოპერაციის განმარტება ამოვაგდე
            //UnOperation = NOT | -

//      pGrammar = new clsGrammar(
//@"
//Program => Operation ; Program | Operation
//Operation => FunctionCall | Assignment
//FunctionCall => Identifier \( ArgumentList \)
//ArgumentList =>  | Expression | Expression , ArgumentList
//Assignment => Value = Expression
//Expression => Value | FunctionCall | Number | String | TRUE | FALSE | \( Expression \) | NOT Expression | Expression BinOperator Expression
//Value => Identifier \. Identifier
//BinOperator => \*\*|[-=+*/%<>]|==|<=|>=|&&|\|\|
//String => (\x22|\x27)((?!\1).|\1{2})*\1
//Number => Int Frac Exp | Int Frac | Int Exp | Int
//Int => -?[1-9][0-9]*
//Frac => [\.][0-9]+
//Exp => [eE][-+]?[0-9]+
//Identifier => [A-Za-z_]\w*
//", "Program");

            //მომავლისთვის Statement => Call | Assignment | increment | newobject | ?decrement?

//      Grammar = new Grammar(
//@"
//Program => Statement Program | Statement
//Statement => FunctionCall Semicolon | AssignmentExpression Semicolon
//FunctionCall => Identifier OpenBracket CloseBracket | Identifier OpenBracket ArgumentList CloseBracket
//ArgumentList => AdditiveExpression Comma ArgumentList | AdditiveExpression
//AssignmentExpression => ExpressionName Equals AdditiveExpression
//AdditiveExpression => MultiplicativeExpression PlusMinus AdditiveExpression | MultiplicativeExpression
//MultiplicativeExpression => UnaryExpression DivMulMod MultiplicativeExpression | UnaryExpression
//UnaryExpression => PlusMinus Expression | Expression
//Expression => String | ExpressionName | FunctionCall | TRUE | FALSE | Number
//ValueExpression => Equals AdditiveExpression
//ExpressionName => Identifier Dot Identifier
//Semicolon => ;
//Identifier => [A-Za-z_]\w*
//OpenBracket => \(
//CloseBracket => \)
//Comma => ,
//Equals => =
//String => \""[^\""]*\""
//Dot => \.
//PlusMinus => [\+\-]
//DivMulMod => [/\*%]
//TRUE => true
//FALSE => false
//Number => IntNumber Frac Exp | IntNumber Frac | IntNumber Exp | IntNumber
//IntNumber => [0-9]*
//Frac => [\.][0-9]+
//Exp => [eE][-+]?[0-9]+
//", "Program");

//      pGrammar = new clsGrammar(
//@"
//Program => Statement ; Program | Statement
//Statement => FunctionCall | AssignmentExpression
//FunctionCall => Identifier \( ArgumentList \)
//ArgumentList =>  | Expression | Expression , ArgumentList
//AssignmentExpression => ExpressionName = Expression
//Expression => AssignmentExpression | ExpressionName | FunctionCall | Number | String | \( Expression \) | LogicalExpression | SumExpression | StringConcatenateExpression | MultipleExpression | ExponentalExpression
//LogicalExpression => TRUE | FALSE | NOT LogicalExpression | LogicalOperand BinLogicalOperator LogicalOperand
//LogicalOperand => LogicalExpression | CompareExpression
//CompareExpression => Expression [<>]==|<=|>=|!= Expression
//StringConcatenateExpression => String \+ String
//SumExpression => SumOperand [-+] SumOperand
//SumOperand => SumExpression | MultipleExpression | Number | ExpressionName
//MultipleExpression => MultipleOperand [*/%] MultipleOperand
//MultipleOperand => MultipleExpression | ExponentalExpression | Number | ExpressionName
//ExponentalExpression => ExponentalOperand \*\* ExponentalOperand
//ExponentalOperand => ExponentalExpression | Number | ExpressionName
//ExpressionName => Identifier \. Identifier
//BinLogicalOperator => &&|\|\|
//String => (\x22|\x27)((?!\1).|\1{2})*\1
//Number => Int Frac Exp | Int Frac | Int Exp | Int
//Int => -?[1-9][0-9]*
//Frac => [\.][0-9]+
//Exp => [eE][-+]?[0-9]+
//Identifier => [A-Za-z_]\w*
//", "Program");
        }
    }
}