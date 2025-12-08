using System;
using System.Collections.Generic;
using UnityEngine;

public class RSA : MonoBehaviour // ← Changé de RSAEncryption à RSA
{
    // Clés stockées
    private int p, q, e;
    private Tuple<int, int> clePublique;
    private Tuple<int, int> clePrivee;
    
    public string PublicKeyString { get; private set; }
    public string PrivateKeyString { get; private set; }

    void Awake()
    {
        GenerateKeys();
    }

    // ----- Génération des clés -----
    public void GenerateKeys()
    {
        Debug.Log("Génération des clés RSA...");
        var cle = ChoixCle(100, 900);

        if (cle == null)
        {
            Debug.LogError("Erreur lors de la génération des clés.");
            return;
        }

        p = cle.Item1;
        q = cle.Item2;
        e = cle.Item3;

        clePublique = ClePublique(p, q, e);
        clePrivee = ClePrivee(p, q, e);

        // Sérialisation des clés en string pour le réseau
        PublicKeyString = $"{clePublique.Item1},{clePublique.Item2}";
        PrivateKeyString = $"{clePrivee.Item1},{clePrivee.Item2}";

        Debug.Log($"Clés générées : p={p}, q={q}, e={e}");
        Debug.Log($"Clé publique (n={clePublique.Item1}, e={clePublique.Item2})");
    }

    // ----- Affichage des clés -----
    public void ShowKeys()
    {
        Debug.Log("===== AFFICHAGE DES CLÉS RSA =====");
        Debug.Log($"Nombres premiers : p = {p}, q = {q}");
        Debug.Log($"Exposant public  : e = {e}");
        Debug.Log($"Clé publique     : (n={clePublique.Item1}, e={clePublique.Item2})");
        Debug.Log($"Clé privée       : (n={clePrivee.Item1}, d={clePrivee.Item2})");
        Debug.Log($"Format réseau (publique) : {PublicKeyString}");
        Debug.Log("==================================");
    }

    // ----- Chiffrement avec clé publique d'un autre joueur -----
    public string Encrypt(string message, string publicKeyString)
    {
        try
        {
            // Parse la clé publique
            string[] parts = publicKeyString.Split(',');
            int n = int.Parse(parts[0]);
            int e_pub = int.Parse(parts[1]);
            Tuple<int, int> cle = new Tuple<int, int>(n, e_pub);

            // Convertit le message en bytes puis en int
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            List<int> encryptedValues = new List<int>();

            foreach (byte b in messageBytes)
            {
                int encrypted = CodageRSA(b, cle);
                encryptedValues.Add(encrypted);
            }

            // Convertit en string
            return string.Join(";", encryptedValues);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erreur de chiffrement : {ex.Message}");
            return null;
        }
    }

    // ----- Déchiffrement avec sa clé privée -----
    public string Decrypt(string encryptedMessage)
    {
        try
        {
            // Parse le message chiffré
            string[] parts = encryptedMessage.Split(';');
            List<byte> decryptedBytes = new List<byte>();

            foreach (string part in parts)
            {
                int encryptedValue = int.Parse(part);
                int decrypted = DechiffrageRSA(encryptedValue, clePrivee);
                decryptedBytes.Add((byte)decrypted);
            }

            return System.Text.Encoding.UTF8.GetString(decryptedBytes.ToArray());
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erreur de déchiffrement : {ex.Message}");
            return "[Message non déchiffrable]";
        }
    }

    // ----- Test RSA (méthode de démonstration) -----
    public void TestRSA()
    {
        Debug.Log("----- TEST CRYPTAGE RSA -----");
        int msg = 42;

        Debug.Log($"Message original : {msg}");
        Debug.Log($"Clé publique  : (n={clePublique.Item1}, e={clePublique.Item2})");
        Debug.Log($"Clé privée    : (n={clePrivee.Item1}, d={clePrivee.Item2})");

        int crypte = CodageRSA(msg, clePublique);
        Debug.Log($"Message crypté : {crypte}");

        int decrypte = DechiffrageRSA(crypte, clePrivee);
        Debug.Log($"Message décrypté : {decrypte}");

        if (decrypte == msg)
            Debug.Log("✓ RSA fonctionne correctement !");
        else
            Debug.LogError("✗ Erreur : le message décrypté ne correspond pas.");
    }

    // ----- Test avec chaîne de caractères -----
    public void TestStringEncryption(string testMessage = "Bonjour!")
    {
        Debug.Log("----- TEST CHIFFREMENT DE TEXTE -----");
        Debug.Log($"Message original : {testMessage}");

        string encrypted = Encrypt(testMessage, PublicKeyString);
        Debug.Log($"Message chiffré : {encrypted}");

        string decrypted = Decrypt(encrypted);
        Debug.Log($"Message déchiffré : {decrypted}");

        if (decrypted == testMessage)
            Debug.Log("✓ Chiffrement de texte fonctionne !");
        else
            Debug.LogError("✗ Erreur de chiffrement de texte.");
    }

    // ----- Utilitaires (votre code original) -----

    static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    static int InverseModulaire(int e, int m)
    {
        int a0 = 1, a1 = 0;
        int b0 = 0, b1 = 1;
        int aa = e, bb = m;

        while (bb != 0)
        {
            int q = aa / bb;

            int temp = aa;
            aa = bb;
            bb = temp % bb;

            temp = a0;
            a0 = a1;
            a1 = temp - q * a1;

            temp = b0;
            b0 = b1;
            b1 = temp - q * b1;
        }

        if (aa != 1)
            return -1;

        if (a0 < 0) a0 += m;

        return a0;
    }

    static bool EstPremier(int p)
    {
        if (p < 2) return false;

        for (int i = 2; i * i <= p; i++)
        {
            if (p % i == 0)
                return false;
        }
        return true;
    }

    static int PremierAleatoire(int inf, int ig)
    {
        List<int> liste = new List<int>();
        for (int i = inf; i <= inf + ig; i++)
            if (EstPremier(i))
                liste.Add(i);

        if (liste.Count == 0)
            return -1;

        System.Random rnd = new System.Random();
        return liste[rnd.Next(liste.Count)];
    }

    static int PremierAleatoireAvec(int n)
    {
        List<int> liste = new List<int>();

        for (int i = 2; i < n; i++)
            if (Gcd(n, i) == 1)
                liste.Add(i);

        if (liste.Count == 0)
            return -1;

        System.Random rnd = new System.Random();
        return liste[rnd.Next(liste.Count)];
    }

    static int ExpoModulaire(long a, long n, long m)
    {
        long result = 1;
        a %= m;

        while (n > 0)
        {
            if ((n & 1) == 1)
                result = (result * a) % m;

            a = (a * a) % m;
            n >>= 1;
        }

        return (int)result;
    }

    // ----- Génération des clés (votre code original) -----

    static Tuple<int, int, int> ChoixCle(int inf, int ig)
    {
        int p = PremierAleatoire(inf, ig);
        int q = PremierAleatoire(p + 1, p + 1 + ig);

        if (p == -1 || q == -1 || p == q)
            return null;

        int phi = (p - 1) * (q - 1);

        int e = PremierAleatoireAvec(phi);
        if (e == -1)
            return null;

        return new Tuple<int, int, int>(p, q, e);
    }

    static Tuple<int, int> ClePublique(int p, int q, int e)
    {
        return new Tuple<int, int>(p * q, e);
    }

    static Tuple<int, int> ClePrivee(int p, int q, int e)
    {
        int phi = (p - 1) * (q - 1);
        int d = InverseModulaire(e, phi);
        return new Tuple<int, int>(p * q, d);
    }

    // ----- RSA (votre code original) -----

    static int CodageRSA(int M, Tuple<int, int> cle)
    {
        return ExpoModulaire(M, cle.Item2, cle.Item1);
    }

    static int DechiffrageRSA(int C, Tuple<int, int> cle)
    {
        return ExpoModulaire(C, cle.Item2, cle.Item1);
    }
}