using UnityEngine;
using UnityEngine.InputSystem; // Asegúrate de tener el Input System instalado
using TMPro;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public Board board;
    public TMP_InputField codeInput;
    public KeyCode executionKey = KeyCode.Alpha1; 
    
    private void Update()
    {
        
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ExecuteCode();
        }
    }

    public void ExecuteCode()
    {
        try
        {
            if (string.IsNullOrEmpty(codeInput.text)) 
            {
                Debug.LogWarning("No hay código para ejecutar");
                return;
            }
            
            List<Token> tokens = new Lexer().Tokenize(codeInput.text);
            List<ASTNode> ast = new Parser(tokens).Parse();
            new Interpreter(board).Execute(ast);
            
            Debug.Log("¡Código ejecutado correctamente!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
            
        }
    }
}
