<script setup>
import { defineEmits } from "vue";

const props = defineProps({
    formData: {
        type: Object,
        required: true,
    },
});

const emit = defineEmits(["update:formData"]);

const updateFormData = (key, value) => {
    const updatedFormData = { ...props.formData };
    updatedFormData[key] = value;
    emit("update:formData", updatedFormData);
};
</script>

<template>
    <div class="form-section">
        <h4 class="section-title">
            Thông số kỹ thuật
            <span class="optional-badge">Không bắt buộc</span>
        </h4>

        <div class="form-row">
            <div class="form-group">
                <label for="warranty">Bảo hành (tháng)</label>
                <input
                    type="number"
                    id="warranty"
                    :value="formData.warranty"
                    @input="updateFormData('warranty', $event.target.value)"
                    min="0"
                    max="60"
                    placeholder="12"
                />
            </div>

            <div class="form-group">
                <label for="ram">RAM (GB)</label>
                <input
                    type="number"
                    id="ram"
                    :value="formData.ram"
                    @input="updateFormData('ram', $event.target.value)"
                    min="1"
                    max="32"
                    placeholder="4"
                />
            </div>

            <div class="form-group">
                <label for="storage">Bộ nhớ trong (GB)</label>
                <input
                    type="number"
                    id="storage"
                    :value="formData.storage"
                    @input="updateFormData('storage', $event.target.value)"
                    min="8"
                    max="2048"
                    placeholder="64"
                />
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label for="screenSize">Kích thước màn hình (inch)</label>
                <input
                    type="number"
                    id="screenSize"
                    :value="formData.screenSize"
                    @input="updateFormData('screenSize', $event.target.value)"
                    min="3"
                    max="15"
                    step="0.1"
                    placeholder="6.1"
                />
            </div>

            <div class="form-group">
                <label for="screenResolution">Độ phân giải màn hình</label>
                <input
                    type="text"
                    id="screenResolution"
                    :value="formData.screenResolution"
                    @input="
                        updateFormData('screenResolution', $event.target.value)
                    "
                    placeholder="1920x1080"
                />
                <span class="input-hint">Định dạng: 1920x1080</span>
            </div>

            <div class="form-group">
                <label for="battery">Dung lượng pin (mAh)</label>
                <input
                    type="number"
                    id="battery"
                    :value="formData.battery"
                    @input="updateFormData('battery', $event.target.value)"
                    min="1000"
                    max="10000"
                    placeholder="4000"
                />
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label for="os">Hệ điều hành</label>
                <input
                    type="text"
                    id="os"
                    :value="formData.os"
                    @input="updateFormData('os', $event.target.value)"
                    placeholder="Android 13"
                />
            </div>

            <div class="form-group">
                <label for="processor">Chip xử lý</label>
                <input
                    type="text"
                    id="processor"
                    :value="formData.processor"
                    @input="updateFormData('processor', $event.target.value)"
                    placeholder="Snapdragon 8 Gen 2"
                />
            </div>

            <div class="form-group">
                <label for="simSlots">Số khe SIM</label>
                <div class="select-wrapper">
                    <select
                        id="simSlots"
                        :value="formData.simSlots"
                        @change="
                            updateFormData('simSlots', $event.target.value)
                        "
                    >
                        <option value="1">1 SIM</option>
                        <option value="2">2 SIM</option>
                        <option value="3">3 SIM</option>
                        <option value="4">4 SIM</option>
                    </select>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.form-section {
    background-color: #fff;
    border-radius: 12px;
    padding: 1.5rem;
    margin-bottom: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.section-title {
    font-size: 1rem;
    margin: 0 0 1rem 0;
    color: #333;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.optional-badge {
    background-color: #f3f4f6;
    color: #6b7280;
    font-size: 0.7rem;
    padding: 0.2rem 0.5rem;
    border-radius: 12px;
    font-weight: normal;
}

.form-row {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 1rem;
    margin-bottom: 1rem;
}

.form-group {
    margin-bottom: 1rem;
}

.form-group label {
    display: block;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
    color: #4b5563;
}

.form-group input,
.form-group select {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    transition: all 0.3s;
}

.form-group input:focus,
.form-group select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.input-hint {
    display: block;
    font-size: 0.8rem;
    color: #6b7280;
    margin-top: 0.3rem;
}

.select-wrapper {
    position: relative;
}

.select-wrapper:after {
    content: "\f078";
    font-family: "Font Awesome 5 Free";
    font-weight: 900;
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    pointer-events: none;
    color: #6b7280;
    font-size: 0.8rem;
}

select {
    appearance: none;
}
</style>
