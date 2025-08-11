// src/modules/users/VerificationScreen.js
import React, { useState } from "react";
import { View, Text, TextInput, TouchableOpacity, StyleSheet } from "react-native";

export default function VerificationScreen({ navigation }) {
  const [code, setCode] = useState("");

  const handleVerify = () => {
    // Verify code with API
    navigation.navigate("Dashboard"); // Example navigation
  };

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Verify Your Login</Text>
      <Text style={styles.subtitle}>Enter the 6-digit code sent to your email or phone</Text>

      <TextInput
        style={styles.input}
        placeholder="Verification Code"
        keyboardType="numeric"
        value={code}
        onChangeText={setCode}
      />

      <TouchableOpacity style={styles.button} onPress={handleVerify}>
        <Text style={styles.buttonText}>Verify</Text>
      </TouchableOpacity>

      <TouchableOpacity>
        <Text style={styles.link}>Resend Code</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 20, justifyContent: "center", backgroundColor: "#fff" },
  title: { fontSize: 22, fontWeight: "bold", marginBottom: 8, textAlign: "center" },
  subtitle: { fontSize: 14, color: "#666", marginBottom: 20, textAlign: "center" },
  input: { borderWidth: 1, borderColor: "#ccc", padding: 10, borderRadius: 5, marginBottom: 15, textAlign: "center" },
  button: { backgroundColor: "#f59e0b", padding: 12, borderRadius: 5, alignItems: "center" },
  buttonText: { color: "#fff", fontWeight: "bold" },
  link: { color: "#f59e0b", textAlign: "center", marginTop: 10 }
});
