# Numeric Regular Expression
A regular expression language for working with numerical sets.

# Usage

Example:
```
new NumericRegex<double>(@"[-10%:20%;mavgasc 2]{5%:50%;2:50}[-20%:10%;mavgdesc 3]{-150%:-1%;2:50}").matches(setOfDoubles)
```

# Language Model
```
   <Main> : <Commands> 
           | <StartAnchor> <Commands> 

    <StartAnchor>: '^' 
    <EndAnchor>    : '$' 
   
   <Commands>: <Number> ' ' <Commands> 
                | <Integer> ' ' <Commands> 
                | <Interval> <Commands> 
                | <Number> <Limiter> <Commands> 
                | <Integer>  <Limiter>  <Commands> 
                | '(' <Commands> ')' <Commands> 
                | '(' <Commands> ')' <Limiter> <Commands> 
                | <Verifier>  <Commands>
                | <Verifier>  <Limiter>  <Commands>
                | <EndAnchor> <EOF> 
                | <EOF> 

    <Interval>: '[' <IntervaNumber> ':' <IntervaNumber> ']' 
                | '[' <IntervaNumber> ':' <IntervaNumber> ']'  <Limiter>
                | '[' <IntervaNumber> ':' <IntervaNumber> ';' <Function> ']' 
                | '[' <IntervaNumber> ':' <IntervaNumber> ';' <Function> ']'  <Limiter> 
 
    <IntervaNumber> : <Number> 
                    | <Integer> 
                    | <Percent> 
     
    <Limiter>: <quantifiers> 
                | ‘{’ <NumericLimiter> ‘}’
                | ‘{’ <PercentageLimiter> ‘}’
                | ‘{’ <PercentageLimiter> ';' <NumericLimiter>  ‘}’

    <quantifiers>       : '*' 
                        | '+' 
                        | '?' 

   <Verifier>  : ‘=’ <IntervaNumber>
                | ‘>’ <IntervaNumber>
	| ‘>=’ <IntervaNumber>
	| ‘<’ <IntervaNumber>
	| ‘<=’ <IntervaNumber>

    <NumericLimiter>: <Integer> 
                            | <Integer> ':' <Integer> 

    <PercentageLimiter>: <Percent> 
                        | <Percent> ':' <Percent> 

    <Function>: <FunctionName> 
                : <FunctionName> <Parameters> 

    <Parameters> : <Number>  
                 | <Integer> 
                 | <Number> <Parameters> 
                 | <Integer> <Parameters> 

   <FunctionName> : [a-zA-Z][a-zA-Z0-9_]* 
   
   <Integer>    : [0-9]+ 
                 | -[0-9]+ 

    <Number>     : [0-9]+[,.][0-9]+ 
                 | -[0-9]+[,.][0-9]+ 
   
   <Percent>    : <Number> '%' 
                 | <Integer> '%' 
```
