using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortLayout : MonoBehaviour
{
    public SpriteRenderer[] spriteRenderers;

    public void SetSortOrder()      //�׸��� ���ĺ����� �ʰ� ���̾� ����
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        int sOrd = 0;
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var tSR = spriteRenderers[i];
            if (tSR.GetComponent<Card>() != null)
            {
                sOrd += 3;
                tSR.sortingOrder = sOrd;
                continue;
            }
            switch (tSR.gameObject.name)
            {
                case "back":
                    tSR.sortingOrder = sOrd + 2;
                    break;
                case "face":
                default:
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }
}
