<template>
	<div class="discount-header">
		<div class="search-container">
			<div class="search-input-container">
				<i class="fas fa-search search-icon"></i>
				<input
					type="text"
					placeholder="Tìm kiếm mã giảm giá..."
					v-model="localSearchQuery"
					@input="emitSearch"
					class="search-input"
				/>
				<button
					v-if="localSearchQuery"
					@click="clearSearch"
					class="clear-button"
				>
					<i class="fas fa-times"></i>
				</button>
			</div>
		</div>
		<div class="actions">
			<button class="add-button" @click="$emit('add')">
				<i class="fas fa-plus"></i> Thêm mã giảm giá
			</button>
			<button
				class="refresh-button"
				@click="$emit('refresh')"
				:disabled="loading"
			>
				<i class="fas fa-sync-alt" :class="{ 'fa-spin': loading }"></i>
			</button>
		</div>
	</div>
</template>

<script setup>
import { ref, watch } from "vue";

const props = defineProps({
	loading: {
		type: Boolean,
		default: false,
	},
	searchQuery: {
		type: String,
		default: "",
	},
});

const emit = defineEmits(["search", "add", "refresh"]);

const localSearchQuery = ref(props.searchQuery);

// Watch for external changes to searchQuery
watch(
	() => props.searchQuery,
	(newVal) => {
		localSearchQuery.value = newVal;
	}
);

const emitSearch = () => {
	emit("search", localSearchQuery.value);
};

const clearSearch = () => {
	localSearchQuery.value = "";
	emit("search", "");
};
</script>

<style scoped>
.discount-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 1.5rem;
	width: 100%;
}

.search-container {
	flex: 1;
	max-width: 500px;
}

.search-input-container {
	position: relative;
	width: 100%;
}

.search-icon {
	position: absolute;
	left: 12px;
	top: 50%;
	transform: translateY(-50%);
	color: #aaa;
}

.search-input {
	width: 100%;
	padding: 10px 10px 10px 38px;
	border: 1px solid #e1e1e1;
	border-radius: 8px;
	font-size: 14px;
	background-color: white;
	transition: all 0.2s;
}

.search-input:focus {
	border-color: var(--primary-color);
	box-shadow: 0 0 0 2px rgba(var(--primary-rgb), 0.2);
	outline: none;
}

.clear-button {
	position: absolute;
	right: 12px;
	top: 50%;
	transform: translateY(-50%);
	background: none;
	border: none;
	color: #aaa;
	cursor: pointer;
	font-size: 14px;
	padding: 0;
}

.clear-button:hover {
	color: #666;
}

.actions {
	display: flex;
	align-items: center;
	gap: 12px;
}

.add-button {
	display: flex;
	align-items: center;
	gap: 6px;
	padding: 10px 16px;
	background-color: var(--primary-color);
	color: white;
	border: none;
	border-radius: 8px;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.2s;
	position: relative;
	z-index: 5;
}

.add-button:hover {
	background-color: var(--primary-hover);
}

.refresh-button {
	width: 38px;
	height: 38px;
	border-radius: 8px;
	border: 1px solid #e1e1e1;
	background-color: white;
	color: #666;
	cursor: pointer;
	display: flex;
	align-items: center;
	justify-content: center;
	transition: all 0.2s;
}

.refresh-button:hover {
	background-color: #f5f5f5;
	color: var(--primary-color);
}

.refresh-button:disabled {
	cursor: not-allowed;
	opacity: 0.7;
}

.fa-spin {
	animation: spin 1s infinite linear;
}

@keyframes spin {
	0% {
		transform: rotate(0deg);
	}
	100% {
		transform: rotate(360deg);
	}
}
</style>
