using System.Collections.Generic;
using UnityEngine;
using System;

public class Parser
{
    private List<Token> tokens;
    private int currentTokenIndex;
    private Token currentToken;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        this.currentTokenIndex = 0;
        this.currentToken = tokens.Count > 0 ? tokens[0] : null;
    }

    private void Advance()
    {
        currentTokenIndex++;
        currentToken = currentTokenIndex < tokens.Count ? tokens[currentTokenIndex] : null;
    }

    private void Expect(string expectedType, string errorMessage)
    {
        if (currentToken == null)
            throw new System.Exception($"{errorMessage}. Se encontró el final del código.");
        
        if (currentToken.Type != expectedType)
            throw new System.Exception($"{errorMessage}. Se encontró '{currentToken.Value}' (Tipo: {currentToken.Type}).");
        
        Advance();
    }

    public List<ASTNode> Parse()
    {
        List<ASTNode> astNodes = new List<ASTNode>();

        // Valida que el primer comando sea Spawn
        if (currentToken == null || currentToken.Value != "Spawn")
            throw new System.Exception("El código debe comenzar con 'Spawn(x, y)'");

        while (currentToken != null)
        {
            ASTNode node = ParseStatement();
            if (node != null) astNodes.Add(node);
        }

        return astNodes;
    }
    private ExpressionNode ParseComparison()
    {
        ExpressionNode left = ParseExpression();
        string op = currentToken.Value; 
        Advance();
        ExpressionNode right = ParseExpression();
        return new ComparisonNode { Left = left, Operator = op, Right = right };
    }
    private void ValidateFunctionArgs(string funcName, int expected, int actual) {
        if (actual != expected) throw new System.Exception($"{funcName} necesita {expected} argumentos");
    }

    private ASTNode ParseStatement()
    {
        switch (currentToken.Type)
        {
            case "METHOD": return ParseCommand();
            case "IDENTIFIER": return ParseAssignment();
            case "LABEL": return ParseLabel();
            case "GOTO": return ParseGoTo();
            default:
                throw new System.Exception($"Instrucción no válida: {currentToken.Value}");
        }
    }

    private ASTNode ParseCommand()
    {
        string commandName = currentToken.Value;
        Advance();

        switch (commandName)
        {
            case "Spawn": return ParseSpawn();
            case "Color": return ParseColor();
            case "Size": return ParseSize();
            case "DrawLine": return ParseDrawLine();
            case "DrawCircle": return ParseDrawCircle();
            case "DrawRectangle": return ParseDrawRectangle();
            case "Fill": return ParseFill();
            default:
                throw new System.Exception($"Comando no reconocido: {commandName}");
        }
    }

    #region Parsers de Comandos
    private SpawnNode ParseSpawn()
{
    Expect("PARENTHESIS", "Se esperaba '(' después de Spawn");
    
    ExpressionNode x = ParseExpression(); 
    Expect("COMMA", "Se esperaba ',' entre coordenadas");
    
    ExpressionNode y = ParseExpression(); 
    
    Expect("PARENTHESIS", "Se esperaba ')' al final de Spawn");
    return new SpawnNode { X = x, Y = y };
}

    private ColorNode ParseColor()
    {
        Expect("PARENTHESIS", "Se esperaba '(' después de Color");
        string color = currentToken.Value;
        Expect("STRING", "Se esperaba un color (ej: \"Red\")");
        Expect("PARENTHESIS", "Se esperaba ')' al final de Color");
        return new ColorNode { Color = color.Trim('"') };
    }

    private SizeNode ParseSize()
    {
        Expect("PARENTHESIS", "Se esperaba '(' después de Size");
        ExpressionNode sizeExpr = ParseExpression();
        Expect("PARENTHESIS", "Se esperaba ')' al final de Size");
        return new SizeNode { Size = sizeExpr };
    }

    private DrawLineNode ParseDrawLine()
    {
        Expect("PARENTHESIS", "Se esperaba '(' después de DrawLine");
        ExpressionNode dirX = ParseExpression();
        Expect("COMMA", "Se esperaba ',' después de dirX");
        ExpressionNode dirY = ParseExpression();
        Expect("COMMA", "Se esperaba ',' después de dirY");
        ExpressionNode distance = ParseExpression();
        Expect("PARENTHESIS", "Se esperaba ')' al final de DrawLine");
        return new DrawLineNode { DirX = dirX, DirY = dirY, Distance = distance };
    }

    private DrawCircleNode ParseDrawCircle()
    {
        Expect("PARENTHESIS", "Se esperaba '(' después de DrawCircle");
        ExpressionNode dirX = ParseExpression();
        Expect("COMMA", "Se esperaba ',' después de dirX");
        ExpressionNode dirY = ParseExpression();
        Expect("COMMA", "Se esperaba ',' después de dirY");
        ExpressionNode radius = ParseExpression();
        Expect("PARENTHESIS", "Se esperaba ')' al final de DrawCircle");
        return new DrawCircleNode { DirX = dirX, DirY = dirY, Radius = radius };
    }

    private DrawRectangleNode ParseDrawRectangle()
    {
        Expect("PARENTHESIS", "Se esperaba '(' después de DrawRectangle");
        ExpressionNode dirX = ParseExpression();
        Expect("COMMA", "Se esperaba ',' después de dirX");
        ExpressionNode dirY = ParseExpression();
        Expect("COMMA", "Se esperaba ',' después de dirY");
        ExpressionNode distance = ParseExpression();
        Expect("COMMA", "Se esperaba ',' después de distance");
        ExpressionNode width = ParseExpression();
        Expect("COMMA", "Se esperaba ',' después de width");
        ExpressionNode height = ParseExpression();
        Expect("PARENTHESIS", "Se esperaba ')' al final de DrawRectangle");
        return new DrawRectangleNode { 
            DirX = dirX, DirY = dirY, Distance = distance, 
            Width = width, Height = height 
        };
    }

    private FillNode ParseFill()
    {
        Expect("PARENTHESIS", "Se esperaba '(' después de Fill");
        Expect("PARENTHESIS", "Se esperaba ')' al final de Fill");
        return new FillNode();
    }
    #endregion

    #region Parsers de Expresiones
    private ExpressionNode ParseExpression() {
    ExpressionNode left = ParseTerm(); 

    
    while (currentToken != null && IsComparisonOperator(currentToken.Value)) {
        string op = currentToken.Value;
        Advance();
        ExpressionNode right = ParseTerm();
        left = new ComparisonNode { Left = left, Operator = op, Right = right };
    }

    return left;
}

private bool IsComparisonOperator(string op) {
    return op == "==" || op == "!=" || op == ">" || op == "<" || op == ">=" || op == "<=";
}

    private ExpressionNode ParseTerm()
    {
        ExpressionNode left = ParseFactor();

        while (currentToken != null && (currentToken.Value == "*" || currentToken.Value == "/" || currentToken.Value == "%"))
        {
            string op = currentToken.Value;
            Advance();
            ExpressionNode right = ParseFactor();
            left = new BinaryOperationNode { Left = left, Operator = op, Right = right };
        }

        return left;
    }

    private ExpressionNode ParseFactor()
    {
        if (currentToken.Type == "NUMBER")
        {
            var node = new NumberNode { Value = int.Parse(currentToken.Value) };
            Advance();
            return node;
        }
        else if (currentToken.Type == "PARENTHESIS" && currentToken.Value == "(")
        {
            Advance();
            ExpressionNode expr = ParseExpression();
            Expect("PARENTHESIS", "Se esperaba ')'");
            return expr;
        }
        else if (currentToken.Type == "METHOD")
        {
            return ParseFunctionCall();
        }
        else if (currentToken.Type == "IDENTIFIER")
        {
            var node = new VariableNode { Name = currentToken.Value };
            Advance();
            return node;
        }
        else
        {
            throw new System.Exception($"Factor inválido: {currentToken?.Value}");
        }
    }

    private FunctionCallNode ParseFunctionCall()
    {
        string funcName = currentToken.Value;
        Advance();
        Expect("PARENTHESIS", $"Se esperaba '(' después de {funcName}");

        var args = new List<ExpressionNode>();
        if (currentToken.Value != ")")
        {
            do {
                args.Add(ParseExpression());
                if (currentToken.Type == "COMMA") Advance();
            } while (currentToken.Type != "PARENTHESIS" && currentToken != null);
        }

        Expect("PARENTHESIS", $"Se esperaba ')' al final de {funcName}");
        return new FunctionCallNode { FunctionName = funcName, Arguments = args };
    }
    #endregion

    #region Parsers de Control
    private AssignNode ParseAssignment()
    {
        string varName = currentToken.Value;
        Advance();
        Expect("ASSIGNMENT", "Se esperaba '←' en asignación");
        ExpressionNode value = ParseExpression();
        return new AssignNode { Variable = varName, Value = value };
    }

    private LabelNode ParseLabel()
    {
        string labelName = currentToken.Value.Replace(":", "");
        Advance();
        return new LabelNode { Name = labelName };
    }

    private GoToNode ParseGoTo()
    {
        Advance();
        Expect("BRACE", "Se esperaba '[' después de GoTo");
        string label = currentToken.Value;
        Expect("IDENTIFIER", "Se esperaba una etiqueta");
        Expect("BRACE", "Se esperaba ']' después de la etiqueta");
        Expect("PARENTHESIS", "Se esperaba '(' antes de la condición");
        ExpressionNode condition = ParseExpression();
        Expect("PARENTHESIS", "Se esperaba ')' después de la condición");
        return new GoToNode { Label = label, Condition = condition };
    }
    #endregion
}
