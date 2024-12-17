<template>
    <div>
        <h1>Login</h1>
        <button @click="login">Login with LinkedIn</button>
    </div>
</template>

<script setup>
import { useLoginMutation } from '../composables/auth'

const { mutate: login } = useLoginMutation()
</script>
