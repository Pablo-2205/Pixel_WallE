using System;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class Lexer : MonoBehaviour
{
    public TMP_InputField userinput;

    private List<(string Type, string Pattern)> tokenPatterns = new List<(string, string)>
    {
        
        ("METHOD", @"\b(Spawn|Color|Size|DrawLine|DrawCircle|DrawRectangle|Fill|GetActualX|GetActualY|GetCanvasSize|GetColorCount|IsBrushColor|IsBrushSize|IsCanvasColor|GoTo)\b"),
        ("BOOLEAN", @"\b(true|false)\b"),
        ("COLOR", @"\b(Red|Blue|Green|Yellow|Orange|Purple|Black|White|Transparent)\b"),
        
        
        ("COMPARISON", @"==|!=|<=|>=|<|>"),
        ("ASSIGNMENT", @"←|<-"),  
        ("OPERATOR", @"[+\-*/%]"),
        
        
        ("NUMBER", @"\b\d+\b"),
        ("STRING", @"""[^""]*"""),  
        ("PARENTHESIS", @"[()]"),
        ("BRACE", @"[{}]"),
        ("COMMA", @","),
        ("COMMENT", @"//.*|/\*[\s\S]*?\*/"),   
        ("NEWLINE", @"\n"),
        
        
        ("IDENTIFIER", @"\b[a-zA-Z_][a-zA-Z0-9_]*\b"),
        
        
        ("WHITESPACE", @"\s+"),
        ("COMMENT", @"//.*|/\*[\s\S]*?\*/"), 
    };

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            string inputText = userinput.text;  
            try
            {
                List<Token> tokens = Tokenize(inputText);
                DebugTokens(tokens);

                Parser parser = new Parser(tokens);
                parser.Parse();
                Debug.Log("Codigo Valido");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e.Message}");
            }
        }
    }

    public List<Token> Tokenize(string input)
{
    List<Token> tokens = new List<Token>();
    int position = 0;

    while (position < input.Length)
    {
        bool matched = false;
        foreach (var (type, pattern) in tokenPatterns)
        {
            Regex regex = new Regex($"^{pattern}");
            Match match = regex.Match(input.Substring(position));

            if (match.Success)
            {
                if (type != "WHITESPACE")
                {
                    string value = match.Value;
                    
                    
                    if (type == "LABEL")
                    {
                        string labelName = value.TrimEnd(':');
                        if (!Regex.IsMatch(labelName, @"^[a-zA-Z_][a-zA-Z0-9_-]*$"))
                        {
                            throw new Exception($"Etiqueta inválida: '{labelName}'. Debe comenzar con letra o _ y contener solo letras, números, - o _");
                        }
                    }
                    
                    tokens.Add(new Token(type, value));
                }
                position += match.Length;
                matched = true;
                break;
            }
        }

        if (!matched)
            throw new Exception($"Carácter inesperado en posición {position}: '{input[position]}'");
    }

    return tokens;
}

    private void DebugTokens(List<Token> tokens)
    {
        foreach (var token in tokens)
        {
            Debug.Log($"Token: {token.Type}, Valor: {token.Value}");
        }
    }
}

public class Token
{
    public string Type { get; set; }
    public string Value { get; set; }

    public Token(string type, string value)
    {
        Type = type;
        Value = value;
    }
}




   

    

    
    
 

      

    


