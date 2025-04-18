<template>
  <div class = "pagination-container">
    <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1" class="prev-button">
      Previous
    </button>
    <button v-for="page in displayPages" :key="page" @click="changePage(page)" :class="{ active: currentPage === page}">
      {{ page }}
    </button>
    <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages" class="next-button">
      Next
    </button>
  </div>
</template>

<script setup>
  import { ref, onMounted, computed } from 'vue'
  import { max, min } from 'lodash-es'
  import { defineEmits, defineProps } from 'vue';

  const emit = defineEmits(['pageChanged'])
  const changePage = (page) => {
    if(page >=1 && page <= totalPages.value){
      emit('pageChanged', page)
    }
  }
  const props = defineProps({
    currentPage: Number,
    pageSize: Number,
    totalProducts: Number,
  })
  totalPages = computed(() => Math.ceil(totalProducts / pageSize));
  displayPages = computed(() =>{
    const pages = []
    const start = max(1, currentPage - 1)
    const end = min(totalPages, currentPage + 2);
    for(let i = start; i <= end; i++){
      pages.push(i)
    }
    return pages;
  });
</script>

<style scoped>
  .pagination-container {
    display: flex;
    justify-content: center;
    margin-top: 20px;
  }
  button {
    margin: 0 5px;
    padding: 10px 15px;
    border: none;
    background-color: #f0f0f0;
    cursor: pointer;
  }
  .prev-button, .next-button {
    background-color: var(--primary-color);
    color: white;
    border: none;
    padding: 10px 20px;
    cursor: pointer;
  }
  button.active {
    font-weight: bold;
    background-color: var(--secondary-color);
  }
  button:disabled {
    background-color: #ccc;
    cursor: not-allowed;
  }
  button:hover:not(:disabled) {
    background-color: #e0e0e0;
  }

</style>