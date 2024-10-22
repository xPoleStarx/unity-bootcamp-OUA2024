using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOnTexture : MonoBehaviour
{
    public Texture2D baseTexture;
    public Color clearColor = Color.black;


    void Update() { DoMouseDrawing(); }


    /// <exception cref="Exception"></exception>
    private void DoMouseDrawing()
    {

        if (Camera.main == null) { throw new Exception("Camera yok"); }

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1)) return;

        if (Input.GetMouseButton(1))
        {
            ClearTexture();
            return;
        }

        // Cast a ray into the scene from screenspace where the mouse is.
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Do nothing if we aren't hitting anything.
        if (!Physics.Raycast(mouseRay, out hit)) return;
        // Do nothing if we didn't get hit.
        if (hit.collider.transform != transform) return;

        // Get the UV coordinate that the mouseRay hit
        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= baseTexture.width;
        pixelUV.y *= baseTexture.height;

        // Set the color as white if the lmb is being pressed, black if rmb.
        Color colorToSet = Color.white;

        // Update the texture and apply.
        baseTexture.SetPixel((int)pixelUV.x, (int)pixelUV.y, colorToSet);
        baseTexture.Apply();
    }

    /// <summary>
    /// Clears the entire texture
    /// </summary>
    private void ClearTexture()
    {
        Color[] clearColorArray = new Color[baseTexture.width * baseTexture.height];
        for (int i = 0; i < clearColorArray.Length; i++)
        {
            clearColorArray[i] = clearColor;
        }
        baseTexture.SetPixels(clearColorArray);
        baseTexture.Apply();
    }
}
