using UnityEngine;
using System.Collections.Generic;
using System;

public class Board : MonoBehaviour
{
    
    public GameObject[] colorPrefabs; // Orden: Red, Blue, Green, Yellow, Orange, Purple, Black, White
    public Player player;
    public int gridSize = 10; // Tamano del Tablero 

    public float cellsize = 2.09f;
    
    
    private int[,] pixelColors; // Matriz de índices de color
    private int brushSize = 1;
    private int currentColorIndex = 7; // Blanco por defecto (índice 7)

    void Start()
    {
        InitializeCanvas();
    }

    private void InitializeCanvas()
    {
        pixelColors = new int[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                pixelColors[x, y] = 7; // empiezo en blanco
                Instantiate(colorPrefabs[7], new Vector3(x * cellsize, y * cellsize, 0), Quaternion.identity);
            }
        }
    }

    // ===== Métodos ====== 
    public void Spawn(int x, int y)
    {
        if (!IsInBounds(x, y)) throw new System.Exception("Posición fuera del canvas");
        player.transform.position = new Vector3(x, y, player.transform.position.z);
    }

    public void SetColor(string colorName)
    {
        currentColorIndex = colorName.ToLower() switch
        {
            "red" => 0,
            "blue" => 1,
            "green" => 2,
            "yellow" => 3,
            "orange" => 4,
            "purple" => 5,
            "black" => 6,
            "white" => 7,
            _ => throw new System.Exception($"Color no válido: {colorName}")
        };
    }
    
public int GetColorCount(string color, int x1, int y1, int x2, int y2)
{
    // Validar coordenadas
    if (!IsInBounds(x1, y1) || !IsInBounds(x2, y2))
        return 0;

    
    int minX = Mathf.Min(x1, x2);
    int maxX = Mathf.Max(x1, x2);
    int minY = Mathf.Min(y1, y2);
    int maxY = Mathf.Max(y1, y2);

    // Convertir nombre de color a índice
    int targetColorIndex = ColorNameToIndex(color);
    if (targetColorIndex == -1) return 0; // Color no válido

    // Contar píxeles del color especifico
    int count = 0;
    for (int x = minX; x <= maxX; x++)
    {
        for (int y = minY; y <= maxY; y++)
        {
            if (pixelColors[x, y] == targetColorIndex)
                count++;
        }
    }
    return count;
}


public bool IsBrushColor(string color)
{
    int colorIndex = ColorNameToIndex(color);
    return colorIndex != -1 && currentColorIndex == colorIndex;
}

// Verifica el color de un píxel relativo a su posicion 
public bool IsCanvasColor(string color, int vertical, int horizontal)
{
    // Convertir nombre de color a índice
    int targetColorIndex = ColorNameToIndex(color);
    if (targetColorIndex == -1) return false;

    // Calcular posición 
    int checkX = Mathf.RoundToInt(player.transform.position.x) + horizontal;
    int checkY = Mathf.RoundToInt(player.transform.position.y) + vertical;

    // Verificar límites y color _-_
    return IsInBounds(checkX, checkY) && 
           pixelColors[checkX, checkY] == targetColorIndex;
}

// Método para convertir de color a indice 
private int ColorNameToIndex(string colorName)
{
    return colorName.ToLower() switch
    {
        "red" => 0,
        "blue" => 1,
        "green" => 2,
        "yellow" => 3,
        "orange" => 4,
        "purple" => 5,
        "black" => 6,
        "white" => 7,
        "transparent" => -1, // Caso especial
        _ => -1 // Color no válido
    };
}

    public void SetBrushSize(int size)
    {
        brushSize = Mathf.Max(1, size % 2 == 0 ? size - 1 : size); // llevando a impar
    }

    public void DrawLine(int dirX, int dirY, int distance)
    {
        if (Mathf.Abs(dirX) > 1 || Mathf.Abs(dirY) > 1)
        {
            throw new System.Exception("Direcciones deben ser -1,0,o 1");
        }
        Vector2Int startPos = new Vector2Int(
            Mathf.RoundToInt(player.transform.position.x),
            Mathf.RoundToInt(player.transform.position.y)
        );

        for (int i = 0; i < distance; i++)
        {
            startPos += new Vector2Int(dirX, dirY);
            if (!IsInBounds(startPos.x, startPos.y)) break;
            PaintPixel(startPos.x, startPos.y);
        }
        player.transform.position = new Vector3(startPos.x, startPos.y, player.transform.position.z);
    }
    public void DrawCircle(int dirX, int dirY, int radius)
    {
        // Validar radio 
        if (radius <= 0) throw new Exception("El radio debe ser mayor que 0");

        // Obtener posición actual y calcular centro
        Vector2Int startPos = new Vector2Int(GetActualX(), GetActualY());
        int centerX = startPos.x + dirX * radius;
        int centerY = startPos.y + dirY * radius;

        // Validar que el centro esté dentro del tablero
        if (!IsInBounds(centerX, centerY))
            throw new Exception("El centro del círculo está fuera del canvas");

        // Algoritmo de Bresenham 
        int x = 0;
        int y = radius;
        int d = 3 - 2 * radius;

        // Pintar puntos cardinales primero
        PaintPixel(centerX + radius, centerY);
        PaintPixel(centerX - radius, centerY);
        PaintPixel(centerX, centerY + radius);
        PaintPixel(centerX, centerY - radius);

        while (x <= y)
        {
            if (d < 0)
            {
                d += 4 * x + 6;
            }
            else
            {
                d += 4 * (x - y) + 10;
                y--;
            }
            x++;

            // Pintar los 8 octantes 
            PaintPixel(centerX + x, centerY + y);
            PaintPixel(centerX - x, centerY + y);
            PaintPixel(centerX + x, centerY - y);
            PaintPixel(centerX - x, centerY - y);
            PaintPixel(centerX + y, centerY + x);
            PaintPixel(centerX - y, centerY + x);
            PaintPixel(centerX + y, centerY - x);
            PaintPixel(centerX - y, centerY - x);
        }

        // Mover al player al centro
        Spawn(centerX, centerY);
    }
public void DrawRectangle(int dirX, int dirY, int distance, int width, int height)
{
    // Validar dimensiones
    if (width <= 0 || height <= 0) 
        throw new Exception("Ancho y alto deben ser mayores que 0");

    // Calcular posición central
    Vector2Int startPos = new Vector2Int(GetActualX(), GetActualY());
    int centerX = startPos.x + dirX * distance;
    int centerY = startPos.y + dirY * distance;

    // Calcular esquinas
    int halfWidth = width / 2;
    int halfHeight = height / 2;
    int left = centerX - halfWidth;
    int right = centerX + halfWidth;
    int top = centerY + halfHeight;
    int bottom = centerY - halfHeight;

    // Dibujar bordes
    for (int x = left; x <= right; x++)
    {
        PaintPixel(x, top);    // Borde superior
        PaintPixel(x, bottom); // Borde inferior
    }

    for (int y = bottom; y <= top; y++)
    {
        PaintPixel(left, y);   // Borde izquierdo
        PaintPixel(right, y);  // Borde derecho
    }

    // Mover Wall-E al centro
    Spawn(centerX, centerY);
}

    public void Fill()
    {
        Vector2Int startPos = new Vector2Int(
            Mathf.RoundToInt(player.transform.position.x),
            Mathf.RoundToInt(player.transform.position.y)
        );

        if (!IsInBounds(startPos.x, startPos.y)) return;

        int targetColor = pixelColors[startPos.x, startPos.y];
        if (targetColor == currentColorIndex) return; // No hacer nada si ya es del mismo color

        FloodFill(startPos.x, startPos.y, targetColor);
    }

    public int GetCanvasSize() => gridSize;
    public int GetActualX() => Mathf.RoundToInt(player.transform.position.x);
    public int GetActualY() => Mathf.RoundToInt(player.transform.position.y);

    // ===== Métodos auxiliares =====
    private bool IsInBounds(int x, int y) => x >= 0 && x < gridSize && y >= 0 && y < gridSize;

    private void PaintPixel(int x, int y)
    {
        if (currentColorIndex == 7) return; // No pintar si es transparente

        int halfSize = brushSize / 2;
        for (int dx = -halfSize; dx <= halfSize; dx++)
        {
            for (int dy = -halfSize; dy <= halfSize; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (IsInBounds(nx, ny))
                {
                    pixelColors[nx, ny] = currentColorIndex;
                    UpdatePixelVisual(nx, ny);
                }
            }
        }
    }

    private void FloodFill(int startX, int startY, int targetColor)
    {
        Queue<Vector2Int> pixelsToFill = new Queue<Vector2Int>();
        pixelsToFill.Enqueue(new Vector2Int(startX, startY));

        while (pixelsToFill.Count > 0)
        {
            Vector2Int p = pixelsToFill.Dequeue();
            if (IsInBounds(p.x, p.y) && pixelColors[p.x, p.y] == targetColor)
            {
                pixelColors[p.x, p.y] = currentColorIndex;
                UpdatePixelVisual(p.x, p.y);

                // Agregar vecinos
                pixelsToFill.Enqueue(new Vector2Int(p.x + 1, p.y));
                pixelsToFill.Enqueue(new Vector2Int(p.x - 1, p.y));
                pixelsToFill.Enqueue(new Vector2Int(p.x, p.y + 1));
                pixelsToFill.Enqueue(new Vector2Int(p.x, p.y - 1));
            }
        }
    }

    private void UpdatePixelVisual(int x, int y)
    {
        GameObject pixel = GameObject.Find($"Pixel_{x}_{y}");
        if (pixel == null)
        {
            pixel = new GameObject($"Pixel_{x}_{y}");
            pixel.transform.position = new Vector3(x, y, 0);
            pixel.AddComponent<SpriteRenderer>();
        }

        SpriteRenderer renderer = pixel.GetComponent<SpriteRenderer>();
        renderer.sprite = colorPrefabs[currentColorIndex].GetComponent<SpriteRenderer>().sprite;
    }
}