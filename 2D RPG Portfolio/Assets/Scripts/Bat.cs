using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonsterManager
{
    public Rigidbody2D rb; // ���� ������ٵ�
    public Animator anim; // ���� �ִϸ�����

    public Animator dieAnim; // ��� �� ����Ʈ �ִϸ�����
    public Transform attackPos; // ���ݹ޾��� �� ��밡 ������ ��ġ

    public int nextMoveY; // ������ Y�� �̵� ���� (-1, 0, 1)

    private void Start()
    {
        // �÷��̾� ��ġ �Ҵ�
        targetTransform = GameObject.FindWithTag("Player").transform;

        hp_Bar.SetActive(false); // HP �� ������
    }

    private void FixedUpdate()
    {
        // �������� HP ��
        hp_fill_Lerp.fillAmount = Mathf.Lerp(hp_fill_Lerp.fillAmount, hp_fill.fillAmount, Time.deltaTime * 7.5f);

        // ���Ͱ� �׾��ٸ� ����
        if (isDie)
        {
            return;
        }

        // ���� ���� ����
        if (targetTransform.position.x < transform.position.x)
        {
            // ����
            Vector3 scale = transform.localScale; // ������ ������ ��
            scale.x = -Mathf.Abs(scale.x); // �������� x���� -����
            transform.localScale = scale; // ������ ������ ���� ������

            Vector3 hpScale = hp_Bar.transform.localScale;
            hpScale.x = -Mathf.Abs(hpScale.x);
            hp_Bar.transform.localScale = hpScale; // HP �ٴ� �׻� �������� ��
        }
        else if (targetTransform.position.x > transform.position.x)
        {
            // ������
            Vector3 scale = transform.localScale; // ������ ������ ��
            scale.x = Mathf.Abs(scale.x); // �������� x���� ����
            transform.localScale = scale; // ������ ������ ���� ������

            Vector3 hpScale = hp_Bar.transform.localScale;
            hpScale.x = Mathf.Abs(hpScale.x);
            hp_Bar.transform.localScale = hpScale; // HP �ٴ� �׻� �������� ��
        }

        MonsterAI();
    }

    // �̵� �Լ�
    public virtual void Move()
    {
        // ���Ͱ� �׾��ٸ� �̵� ����
        if (isDie)
        {
            return;
        }

        // �̵�
        rb.velocity = new Vector2(nextMove * moveSpeed, nextMoveY * moveSpeed);
    }
    // ���� �ൿ AI �Լ�
    public override void MonsterAI()
    {
        // �ǰ��� ���� ���̰ų� ���� ���� ��
        if (isHitMonster || isAttack)
        {
            return;
        }

        // �̵�
        Move();

        thinkTime += Time.deltaTime;

        // ���� �ൿ ���� ����
        if (thinkTime >= nextThinkTime)
        {
            thinkTime = 0;

            nextMove = Random.Range(-1, 2); // ���� -1 ~ 1���� ���� �ޱ�
            nextMoveY = Random.Range(-1, 2); // Y�� ���� -1 ~ 1���� ���� �ޱ�

            nextThinkTime = Random.Range(1f, 2.5f); // ���� �ൿ�� ���ϱ���� �ɸ��� �ð� �ޱ�
        }
    }

    // ����� ���� �Լ�
    public override void TakeDamage(int damage, Transform Pos, bool isCritical)
    {
        // �׾��ٸ� ����
        if (isDie)
        {
            return;
        }

        hp_Bar.SetActive(true); // HP �� Ȱ��ȭ

        CancelInvoke("Hide_HpBar"); // �ʱ�ȭ
        Invoke("Hide_HpBar", 2f); // 2�� �� HP �� ��Ȱ��ȭ

        base.TakeDamage(damage, Pos, isCritical); // ����� ����

        // ũ��Ƽ���̶��
        if (isCritical)
        {
            // ũ��Ƽ�� ����� ��� �ؽ�Ʈ ����
            GameObject damage_HudText = ObjectPoolingManager.instance.GetObject("HudText_CriticalDam");
            damage_HudText.transform.position = hudPos.position; // ��� �ؽ�Ʈ ��ġ ����
            damage_HudText.transform.GetChild(0).GetComponent<Damage_HudText>().ShowDamageText(Mathf.RoundToInt(damage * (GameManager.instance.critical_Value + GameManager.instance.increased_CriticalValue))); // ��� �ؽ�Ʈ�� ǥ�� �� ����� ���� (�׸���)
            damage_HudText.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<DamageHudText_Shadow>().ShowDamageText(Mathf.RoundToInt(damage * (GameManager.instance.critical_Value + GameManager.instance.increased_CriticalValue))); // ��� �ؽ�Ʈ�� ǥ�� �� ����� ���� (�ؽ�Ʈ)

            // �˹�
            float x = transform.position.x - Pos.position.x; // �з��� ����
            if (x > 0)
            {
                rb.velocity = new Vector2(5f, rb.velocity.y); // ���������� 5��ŭ �˹� 
            }
            else if (x < 0)
            {
                rb.velocity = new Vector2(-5f, rb.velocity.y); // �������� 5��ŭ �˹� 
            }
        }
        else
        {
            // ����� ��� �ؽ�Ʈ ����
            GameObject damage_HudText = ObjectPoolingManager.instance.GetObject("HudText_Damage");
            damage_HudText.transform.position = hudPos.position; // ��� �ؽ�Ʈ ��ġ ����
            damage_HudText.transform.GetChild(0).GetComponent<Damage_HudText>().ShowDamageText(damage); // ��� �ؽ�Ʈ�� ǥ�� �� ����� ���� (�׸���)
            damage_HudText.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<DamageHudText_Shadow>().ShowDamageText(damage); // ��� �ؽ�Ʈ�� ǥ�� �� ����� ���� (�ؽ�Ʈ)

            // �˹�
            float x = transform.position.x - Pos.position.x; // �з��� ����
            if (x > 0)
            {
                rb.velocity = new Vector2(3f, rb.velocity.y); // ���������� 3��ŭ �˹� 
            }
            else if (x < 0)
            {
                rb.velocity = new Vector2(-3f, rb.velocity.y); // �������� 3��ŭ �˹� 
            }
        }

        hp_fill.fillAmount = health / maxHealth; // ü�¹� ����
    }

    // ��� �Լ�
    public override void Die()
    {
        base.Die();

        anim.SetTrigger("Die"); // �״� �ִϸ��̼�
        hp_Bar.gameObject.SetActive(false); // HP �� ��Ȱ��ȭ
    }

    // ������ ���
    public override void DropItem()
    {
        base.DropItem();

        // ����� ������ ���� ���� ��
        int rand_dropAmount = Random.Range(minItemDrop, maxItemDrop);

        // ����� ������ ������ŭ for�� ����
        for (int i = 0; i < rand_dropAmount; i++)
        {
            float rand_dropItem = Random.Range(0f, 1f);

            if (rand_dropItem < 0.55f) // ����ġ (55%) 
            {
                Debug.Log("����ġ ���");
                GameObject exp = ObjectPoolingManager.instance.GetObject("Item_Exp"); // ������Ʈ Ǯ���� ����ġ �뿩
                exp.transform.position = this.transform.position; // ��ġ �ʱ�ȭ
            }
            else if (rand_dropItem < 0.95f) // ���� (40%) 
            {
                Debug.Log("���� ���");
                GameObject coin = ObjectPoolingManager.instance.GetObject("Item_Coin"); // ������Ʈ Ǯ���� ���� �뿩
                coin.transform.position = this.transform.position; // ��ġ �ʱ�ȭ
            }
            else if (rand_dropItem < 1.0f) // �ݱ� (5%) 
            {
                Debug.Log("�ݱ� ���");
                GameObject bullion = ObjectPoolingManager.instance.GetObject("Item_Bullion"); // ������Ʈ Ǯ���� �ݱ� �뿩
                bullion.transform.position = this.transform.position; // ��ġ �ʱ�ȭ
            }
        }
    }

    // HP�ٸ� ����� �Լ�
    public override void Hide_HpBar()
    {
        base.Hide_HpBar();
    }
}