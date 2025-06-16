using System;
using System.Collections.Generic;
using UnityEngine;

public class ExpressionEvaluator
{
    private readonly Board _board;
    private readonly Dictionary<string, int> _variables = new Dictionary<string, int>();

    public ExpressionEvaluator(Board board)
    {
        _board = board;
    }

    public int Evaluate(ExpressionNode node)
    {
        try
        {
            switch (node)
            {
                case NumberNode numNode:
                    return numNode.Value;

                case ComparisonNode compNode:
                    int leftComp = Evaluate(compNode.Left);
                    int rightComp = Evaluate(compNode.Right);
                    return compNode.Operator switch
                    {
                        "==" => leftComp == rightComp ? 1 : 0,
                        "!=" => leftComp != rightComp ? 1 : 0,
                        ">"  => leftComp > rightComp ? 1 : 0,
                        "<"  => leftComp < rightComp ? 1 : 0,
                        ">=" => leftComp >= rightComp ? 1 : 0,
                        "<=" => leftComp <= rightComp ? 1 : 0,
                        _ => throw new Exception($"Operador de comparación no soportado: '{compNode.Operator}'")
                    };

                case BinaryOperationNode binOpNode:
                    int left = Evaluate(binOpNode.Left);
                    int right = Evaluate(binOpNode.Right);
                    return binOpNode.Operator switch
                    {
                        "+" => left + right,
                        "-" => left - right,
                        "*" => left * right,
                        "/" => (right == 0) ? throw new Exception("División por cero") : left / right,
                        "%" => left % right,
                        _ => throw new Exception($"Operador no soportado: '{binOpNode.Operator}'")
                    };

                case FunctionCallNode funcCallNode:
                    return EvaluateFunctionCall(funcCallNode);

                case VariableNode varNode:
                    return _variables.TryGetValue(varNode.Name, out int value) 
                        ? value 
                        : throw new Exception($"Variable no definida: '{varNode.Name}'");

                case BooleanNode boolNode:
                    return boolNode.Value ? 1 : 0;

                default:
                    throw new Exception($"Nodo no soportado: {node.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al evaluar expresión: {ex.Message}");
            throw;
        }
    }

    private int EvaluateFunctionCall(FunctionCallNode node)
    {
        switch (node.FunctionName)
        {
            case "GetCanvasSize":
                ValidateFunctionArgs("GetCanvasSize", 0, node.Arguments.Count);
                return _board.GetCanvasSize();
                
            case "GetActualX":
                ValidateFunctionArgs("GetActualX", 0, node.Arguments.Count);
                return _board.GetActualX();
                
            case "GetActualY":
                ValidateFunctionArgs("GetActualY", 0, node.Arguments.Count);
                return _board.GetActualY();
                
            case "GetColorCount":
                ValidateFunctionArgs("GetColorCount", 5, node.Arguments.Count);
                string color = EvaluateString(node.Arguments[0]);
                int x1 = Evaluate(node.Arguments[1]);
                int y1 = Evaluate(node.Arguments[2]);
                int x2 = Evaluate(node.Arguments[3]);
                int y2 = Evaluate(node.Arguments[4]);
                return _board.GetColorCount(color, x1, y1, x2, y2);
                
            case "IsBrushColor":
                ValidateFunctionArgs("IsBrushColor", 1, node.Arguments.Count);
                string brushColor = EvaluateString(node.Arguments[0]);
                return _board.IsBrushColor(brushColor) ? 1 : 0;
                
            case "IsCanvasColor":
                ValidateFunctionArgs("IsCanvasColor", 3, node.Arguments.Count);
                string canvasColor = EvaluateString(node.Arguments[0]);
                int vertical = Evaluate(node.Arguments[1]);
                int horizontal = Evaluate(node.Arguments[2]);
                return _board.IsCanvasColor(canvasColor, vertical, horizontal) ? 1 : 0;
                
            default:
                throw new Exception($"Función no reconocida: '{node.FunctionName}'");
        }
    }

    private string EvaluateString(ExpressionNode node)
    {
        if (node is StringNode strNode)
        {
            return strNode.Value;
        }
        throw new Exception($"Se esperaba una cadena de texto, pero se encontró: {node.GetType().Name}");
    }

    private void ValidateFunctionArgs(string funcName, int expected, int actual)
    {
        if (actual != expected)
        {
            throw new Exception($"La función '{funcName}' requiere {expected} argumentos, pero se proporcionaron {actual}");
        }
    }

    public void SetVariable(string name, int value)
    {
        _variables[name] = value;
    }

    public bool TryGetVariable(string name, out int value)
    {
        return _variables.TryGetValue(name, out value);
    }
}
