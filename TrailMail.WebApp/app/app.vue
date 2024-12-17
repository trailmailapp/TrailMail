<script lang="ts">
  import { useWhoAmI } from '~~/composables/auth';
  const { data: user } = useWhoAmI();
</script>

<template>
  <div>
    <NuxtPage v-if="user" />
    <div v-else>
      <p>You must be logged in to access this application.</p>
      <NuxtLink to="/login">Login</NuxtLink>
    </div>
  </div>
</template>