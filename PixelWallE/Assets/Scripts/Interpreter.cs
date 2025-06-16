using System;
using System.Collections.Generic;
using UnityEngine;

public class Interpreter
{
    private readonly Board _board;
    private readonly ExpressionEvaluator _evaluator;
    private readonly Dictionary<string, int> _labelIndices = new Dictionary<string, int>();

    public Interpreter(Board board)
    {
        _board = board;
        _evaluator = new ExpressionEvaluator(board);
    }

    public void Execute(List<ASTNode> nodes)
    {
        RegisterLabels(nodes); // Primera pasada: registrar etiquetas

        for (int i = 0; i < nodes.Count; i++)
        {
            ExecuteNode(nodes[i], nodes, ref i);
        }
    }

    private void RegisterLabels(List<ASTNode> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] is LabelNode labelNode)
            {
                if (_labelIndices.ContainsKey(labelNode.Name))
                    throw new Exception($"Etiqueta duplicada: {labelNode.Name}");
                _labelIndices[labelNode.Name] = i;
            }
        }
    }

    private void ExecuteNode(ASTNode node, List<ASTNode> nodes, ref int currentIndex)
    {
        try
        {
            switch (node)
            {
                case SpawnNode spawn:
                    ExecuteSpawn(spawn);
                    break;
                case ColorNode color:
                    ExecuteColor(color);
                    break;
                case SizeNode size:
                    ExecuteSize(size);
                    break;
                case DrawLineNode drawLine:
                    ExecuteDrawLine(drawLine);
                    break;
                case DrawCircleNode drawCircle:
                    ExecuteDrawCircle(drawCircle);
                    break;
                case DrawRectangleNode drawRectangle:
                    ExecuteDrawRectangle(drawRectangle);
                    break;
                case FillNode fill:
                    ExecuteFill(fill);
                    break;
                case AssignNode assign:
                    ExecuteAssign(assign);
                    break;
                case GoToNode goTo:
                    ExecuteGoTo(goTo, ref currentIndex);
                    break;
                case LabelNode _:
                    break; 
                default:
                    throw new Exception($"Nodo no reconocido: {node.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error en línea {currentIndex + 1}: {ex.Message}");
            throw;
        }
    }

    // ===== Implementación de cada comando =====
    private void ExecuteSpawn(SpawnNode node)
    {
        int x = _evaluator.Evaluate(node.X);
        int y = _evaluator.Evaluate(node.Y);
        _board.Spawn(x, y);
    }

    private void ExecuteColor(ColorNode node)
    {
        _board.SetColor(node.Color);
    }

    private void ExecuteSize(SizeNode node)
    {
        int size = _evaluator.Evaluate(node.Size);
        _board.SetBrushSize(size);
    }

    private void ExecuteDrawLine(DrawLineNode node)
    {
        int dirX = _evaluator.Evaluate(node.DirX);
        int dirY = _evaluator.Evaluate(node.DirY);
        int distance = _evaluator.Evaluate(node.Distance);
        _board.DrawLine(dirX, dirY, distance);
    }

    private void ExecuteDrawCircle(DrawCircleNode node)
    {
        int dirX = _evaluator.Evaluate(node.DirX);
        int dirY = _evaluator.Evaluate(node.DirY);
        int radius = _evaluator.Evaluate(node.Radius);
        _board.DrawCircle(dirX, dirY, radius);
        Debug.Log($"DrawCircle: dir=({dirX},{dirY}), radius={radius}");
    }

    private void ExecuteDrawRectangle(DrawRectangleNode node)
    {
        int dirX = _evaluator.Evaluate(node.DirX);
        int dirY = _evaluator.Evaluate(node.DirY);
        int distance = _evaluator.Evaluate(node.Distance);
        int width = _evaluator.Evaluate(node.Width);
        int height = _evaluator.Evaluate(node.Height);
        _board.DrawRectangle(dirX, dirY, distance, width, height);
        Debug.Log($"DrawRectangle: dir=({dirX},{dirY}), distance={distance}, w={width}, h={height}");
    }

    private void ExecuteFill(FillNode node)
    {
        _board.Fill(); 
    }

    private void ExecuteAssign(AssignNode node)
    {
        int value = _evaluator.Evaluate(node.Value);
        _evaluator.SetVariable(node.Variable, value);
    }

    private void ExecuteGoTo(GoToNode node, ref int currentIndex)
    {
        int conditionValue = _evaluator.Evaluate(node.Condition);
        if (conditionValue != 0 && _labelIndices.TryGetValue(node.Label, out int targetIndex))
        {
            currentIndex = targetIndex - 1;
        }
    }
}