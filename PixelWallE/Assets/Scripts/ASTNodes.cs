using System.Collections.Generic;
using UnityEngine;

public abstract class ASTNode { }

public abstract class ExpressionNode : ASTNode { }  // Hereda de ASTNode

// Nodos de expresiones
public class NumberNode : ExpressionNode
{
    public int Value { get; set; }
}
public class ComparisonNode : ExpressionNode
{
    public ExpressionNode Left { get; set; }   // Expresión izquierda (variable, número, etc.)
    public string Operator { get; set; }       // Operador: "==", "!=", ">", "<", ">=", "<="
    public ExpressionNode Right { get; set; }  // Expresión derecha
}
public class BooleanNode : ExpressionNode
{
    public bool Value { get; set; }
}

// Nodo para cadenas de texto (ej: "Red", "Blue")
public class StringNode : ExpressionNode
{
    public string Value { get; set; }
}

public class BinaryOperationNode : ExpressionNode {  // Corregí el nombre (sin "s")
    public ExpressionNode Left { get; set; }
    public string Operator { get; set; }  // "+", "-", "*", "/", "%"
    public ExpressionNode Right { get; set; }
}

public class FunctionCallNode : ExpressionNode {
    public string FunctionName { get; set; }  // "GetCanvasSize", "GetActualX", etc.
    public List<ExpressionNode> Arguments { get; set; } = new List<ExpressionNode>();
}

public class VariableNode : ExpressionNode {  // Nuevo: para manejar variables (ej: "x")
    public string Name { get; set; }
}

// Nodos de comandos
public class SpawnNode : ASTNode {
    public ExpressionNode X { get; set; }
    public ExpressionNode Y { get; set; }
}

public class ColorNode : ASTNode {
    public string Color { get; set; }  // "Red", "Blue", etc.
}

public class FillNode : ASTNode { }  // Sin parámetros

public class DrawLineNode : ASTNode {
    public ExpressionNode DirX { get; set; }  // Usa ExpressionNode para soportar operaciones
    public ExpressionNode DirY { get; set; }
    public ExpressionNode Distance { get; set; }
}

public class DrawCircleNode : ASTNode {
    public ExpressionNode DirX { get; set; }  // Cambiado a ExpressionNode
    public ExpressionNode DirY { get; set; }
    public ExpressionNode Radius { get; set; }
}

public class DrawRectangleNode : ASTNode {
    public ExpressionNode DirX { get; set; }  // Cambiado a ExpressionNode
    public ExpressionNode DirY { get; set; }
    public ExpressionNode Distance { get; set; }
    public ExpressionNode Width { get; set; }
    public ExpressionNode Height { get; set; }
}

public class SizeNode : ASTNode {
    public ExpressionNode Size { get; set; }  
}

public class AssignNode : ASTNode {
    public string Variable { get; set; }
    public ExpressionNode Value { get; set; }  
}

public class GoToNode : ASTNode {
    public string Label { get; set; }
    public ExpressionNode Condition { get; set; }  
}

public class LabelNode : ASTNode { 
    public string Name { get; set; }
}